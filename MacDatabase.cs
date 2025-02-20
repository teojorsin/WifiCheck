// This file under the MIT license.
// See LICENSE for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Text;

namespace WifiCheck
{
    public class MacDatabase
    {
        private Dictionary<string, int> _LargeBlocks = new Dictionary<string, int>();
        private Dictionary<string, int> _MediumBlocks = new Dictionary<string, int>();
        private Dictionary<string, int> _SmallBlocks = new Dictionary<string, int>();
        private Dictionary<string, int> _Vendors = new Dictionary<string, int>();
        private List<string> _VendorsList = new List<string>();
        private int _NextVendorID = 0;

        public MacDatabase() { }

        public void Clear()
        {
            _LargeBlocks.Clear();
            _MediumBlocks.Clear();
            _SmallBlocks.Clear();
            _Vendors.Clear();
            _VendorsList.Clear();
            _NextVendorID = 0;
        }

        private int TryAddVendor(string orgName)
        {
            int vendorId = -1;
            if (!_Vendors.ContainsKey(orgName))
            {
                _Vendors.Add(orgName, _NextVendorID);
                _VendorsList.Add(orgName);
                vendorId = _NextVendorID;
                _NextVendorID += 1;
            }
            else
            {
                vendorId = _Vendors[orgName];
            }
            return vendorId;
        }

        public bool DownloadMacVendors()
        {
            bool result = true;
            WebClient wc = new WebClient();
            wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36");
            try
            {
                wc.DownloadFile("http://standards.ieee.org/develop/regauth/oui/oui.csv", "oui.csv");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERROR: " + ex.Message);
                if (ex.InnerException != null)
                    Debug.WriteLine("   Inner ex: " + ex.InnerException.Message);
                result = false;
            }
            try
            {
                wc.DownloadFile("http://standards.ieee.org/develop/regauth/oui28/mam.csv", "mam.csv");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERROR: " + ex.Message);
                if (ex.InnerException != null)
                    Debug.WriteLine("   Inner ex: " + ex.InnerException.Message);
                result = false;
            }
            try
            {
                wc.DownloadFile("http://standards.ieee.org/develop/regauth/oui36/oui36.csv", "oui36.csv");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERROR: " + ex.Message);
                if (ex.InnerException != null)
                    Debug.WriteLine("   Inner ex: " + ex.InnerException.Message);
                result = false;
            }

            wc.Dispose();
            wc = null;

            return result;
        }
        public void LoadDefaultCSVs()
        {
            LoadCSV("oui.csv");
            LoadCSV("mam.csv");
            LoadCSV("oui36.csv");
        }

        public bool LoadCSV(string filename)
        {
            string[] data = null;
            Dictionary<string, int> storage = null;
            bool isFirstLine = true;
            bool enclosed = false;

            //field is citation enclosed
            string oui = "";
            string orgName = "";

            try
            {
                data = System.IO.File.ReadAllLines(filename, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }

            //parse lines and add to dictionaries
            foreach (string line in data)
            {
                //skip first line
                if (isFirstLine)
                {
                    isFirstLine = false;
                    continue;
                }

                if (line.StartsWith("MA-L"))
                {
                    //Debug.WriteLine("MA-L")
                    storage = _LargeBlocks;
                    oui = line.Substring(5, 6);
                    enclosed = (line.Substring(12, 1) == "\"");
                    if (enclosed)
                    {
                        orgName = line.Substring(13, line.IndexOf("\"", 14) - 13);
                    }
                    else
                    {
                        orgName = line.Substring(12, line.IndexOf(",", 13) - 12);
                    }
                }
                else if (line.StartsWith("MA-M"))
                {
                    //Debug.WriteLine("MA-M")
                    storage = _MediumBlocks;
                    oui = line.Substring(5, 7);
                    enclosed = (line.Substring(13, 1) == "\"");
                    if (enclosed)
                    {
                        orgName = line.Substring(14, line.IndexOf("\"", 15) - 14);
                    }
                    else
                    {
                        orgName = line.Substring(13, line.IndexOf(",", 14) - 13);
                    }
                }
                else if (line.StartsWith("MA-S"))
                {
                    //				Debug.WriteLine("MA-S")
                    storage = _SmallBlocks;
                    oui = line.Substring(5, 9);
                    enclosed = (line.Substring(15, 1) == "\"");
                    if (enclosed)
                    {
                        orgName = line.Substring(16, line.IndexOf("\"", 17) - 16);
                    }
                    else
                    {
                        orgName = line.Substring(15, line.IndexOf(",", 16) - 15);
                    }
                }

                if (!string.IsNullOrEmpty(oui))
                {
                    oui = oui.ToUpper();
                    if (storage != null && !storage.ContainsKey(oui))
                    {
                        storage.Add(oui, TryAddVendor(orgName));
                    }
                }

            }

            return true;
        }

        public bool SaveToFile(string filename)
        {
            bool result = true;
            StringBuilder data = new StringBuilder(2097152);
            MemoryStream dataStream = null;
            FileStream fileStream = null;
            DeflateStream compressionStream = null;

            foreach (string vend in _VendorsList)
            {
                data.Append("V ").AppendLine(vend);
            }
            foreach (KeyValuePair<string, int> kvp in _SmallBlocks)
            {
                data.Append("S ").AppendLine(kvp.Key.ToString() + " " + kvp.Value.ToString());
            }
            foreach (KeyValuePair<string, int> kvp in _MediumBlocks)
            {
                data.Append("M ").AppendLine(kvp.Key.ToString() + " " + kvp.Value.ToString());
            }
            foreach (KeyValuePair<string, int> kvp in _LargeBlocks)
            {
                data.Append("L ").AppendLine(kvp.Key.ToString() + " " + kvp.Value.ToString());
            }

            //compress
            try
            {
                fileStream = File.Create(filename);
                compressionStream = new System.IO.Compression.DeflateStream(fileStream, CompressionMode.Compress);
                dataStream = new MemoryStream(Encoding.UTF8.GetBytes(data.ToString()));
                dataStream.CopyTo(compressionStream);

                dataStream.Close();
                compressionStream.Close();
                fileStream.Close();

                dataStream.Dispose();
                compressionStream.Dispose();
                fileStream.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERROR saving compressed stream: " + ex.Message);
                result = false;
            }
            finally
            {
                if (dataStream != null)
                {
                    dataStream.Close();
                    dataStream.Dispose();
                }
                if (compressionStream != null)
                {
                    compressionStream.Close();
                    compressionStream.Dispose();
                }
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream.Dispose();
                }
            }
            return result;
        }
        public bool LoadFromFile(string filename)
        {
            bool result = true;
            FileStream fileStream = null;

            try
            {
                fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                result = false;
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream.Dispose();
                }
            }

            if (fileStream != null)
                result = LoadFromStream(fileStream);

            return result;
        }

        public bool LoadFromResource(string name)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Stream stream = asm.GetManifestResourceStream(name);
            return LoadFromCompressedStream(stream);
        }

        public bool LoadFromCompressedStream(Stream input)
        {
            bool result = true;

            MemoryStream dataStream = null;
            DeflateStream decompressionStream = null;

            try
            {
                dataStream = new MemoryStream();
                decompressionStream = new System.IO.Compression.DeflateStream(input, CompressionMode.Decompress);
                decompressionStream.CopyTo(dataStream);

                decompressionStream.Close();
                decompressionStream.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                result = false;

            }
            finally
            {
                if (decompressionStream != null)
                {
                    decompressionStream.Close();
                    decompressionStream.Dispose();
                }
            }
            result = LoadFromStream(dataStream);
            return result;
        }


        public bool LoadFromStream(Stream input)
        {
            bool result = true;
            StreamReader reader = null;
            string oui = null;
            int vendorId = 0;
            string line = null;

            //clear all data first
            Clear();

            try
            {
                input.Position = 0;
                reader = new StreamReader(input, Encoding.UTF8);

                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();

                    if (line.StartsWith("V"))
                    {
                        TryAddVendor(line.Substring(2));

                    }
                    else if (line.StartsWith("S"))
                    {
                        oui = line.Substring(2, 9);
                        vendorId = -1;
                        int.TryParse(line.Substring(12), out vendorId);
                        _SmallBlocks.Add(oui, vendorId);

                    }
                    else if (line.StartsWith("M"))
                    {
                        oui = line.Substring(2, 7);
                        vendorId = -1;
                        int.TryParse(line.Substring(10), out vendorId);
                        _MediumBlocks.Add(oui, vendorId);

                    }
                    else if (line.StartsWith("L"))
                    {
                        oui = line.Substring(2, 6);
                        vendorId = -1;
                        int.TryParse(line.Substring(9), out vendorId);
                        _LargeBlocks.Add(oui, vendorId);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return result;
        }

        public string FindVendor(string mac)
        {
            //search small blocks, then medium and finally large
            int orgId = -1;
            string orgName = "-";
            string mac6 = null;
            string mac7 = null;
            string mac9 = null;

            //process mac adress
            mac = mac.Replace(" ", "");
            mac = mac.Replace(":", "");
            mac = mac.Replace("-", "").ToUpper();

            mac6 = mac.Substring(0, 6);
            mac7 = mac.Substring(0, 7);
            mac9 = mac.Substring(0, 9);

            if (_SmallBlocks.ContainsKey(mac9))
            {
                orgId = _SmallBlocks[mac9];

            }
            else if (_MediumBlocks.ContainsKey(mac7))
            {
                orgId = _MediumBlocks[mac7];

            }
            else if (_LargeBlocks.ContainsKey(mac6))
            {
                orgId = _LargeBlocks[mac6];

            }

            if (orgId > 0 && orgId < _VendorsList.Count)
            {
                orgName = _VendorsList[orgId];
            }

            return orgName;
        }
    }
}