using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Newtonsoft.Json;
using System.Runtime;
using System.IO;
using System.Security.Cryptography;
using System.Security;

namespace TestWCF
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IService1
    {
        string secur = Convert.ToBase64String(Encoding.UTF8.GetBytes("PHEAN@#$"));
        public ResponseData Data(ResponseData s)
        {
            byte[] d = DecryptAES(s.Data, secur, s.stringIV, s.stringSalt);
            string da = Encoding.UTF8.GetString(d);

            Info w = new Info();
            Person p = JsonConvert.DeserializeObject<Person>(da);
            w.stringInformation = p.stringName + "/" + p.stringGender + "/" + p.stringAge;
            
            string inf=JsonConvert.SerializeObject(w);
            string iv = Convert.ToBase64String(GetRandomBytes(16));
            string salt=Convert.ToBase64String(GetRandomBytes(8));
            string data = EncryptAES(Convert.ToBase64String(Encoding.UTF8.GetBytes(inf)),secur, iv, salt);
            ResponseData dt = new ResponseData();
            dt.Data = data;
            dt.stringIV = iv;
            dt.stringSalt = salt;

            return dt;
        }
        public byte[] GetRandomBytes(int saltLength)
        {
            byte[] byteSalt = new byte[saltLength];
            RNGCryptoServiceProvider.Create().GetBytes(byteSalt);
            return byteSalt;
        }
      
        public byte[] DecryptAES(string stringData, string stringPassword, string stringIV, string stringSalt)
        {
            try
            {
                byte[] dataByes = Convert.FromBase64String(stringData);
                byte[] passwordBytes = Convert.FromBase64String(stringPassword);
                byte[] ivBytes = Convert.FromBase64String(stringIV);
                byte[] saltBytes = Convert.FromBase64String(stringSalt);
                byte[] data=null;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (RijndaelManaged AES = new RijndaelManaged())
                    {
                        AES.KeySize = 256;// bit
                        AES.BlockSize = 128;// bit

                        var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 100);
                        AES.Key = key.GetBytes(AES.KeySize / 8);
                        AES.IV = ivBytes;
                        AES.Mode = CipherMode.CBC;

                        using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(dataByes, 0, dataByes.Length);
                            cs.Close();
                        }
                        return ms.ToArray();
                    }
                }
                //return stringDataPlanText;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public string EncryptAES(string stringData, string stringPassword, string stringIV, string stringSalt)
        {
            try
            {
                byte[] dataByes = Convert.FromBase64String(stringData);
                byte[] passwordBytes = Convert.FromBase64String(stringPassword);
                byte[] ivBytes = Convert.FromBase64String(stringIV);
                byte[] saltBytes = Convert.FromBase64String(stringSalt);
                string stringEncrypt = null;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (RijndaelManaged AES = new RijndaelManaged())
                    {
                        AES.KeySize = 256;
                        AES.BlockSize = 128;
                        var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 100);
                        AES.Key = key.GetBytes(AES.KeySize / 8);
                        AES.IV = ivBytes;
                        AES.Mode = CipherMode.CBC;
                        using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(dataByes, 0, dataByes.Length);
                            cs.Close();
                        }
                        stringEncrypt = Convert.ToBase64String(ms.ToArray());
                    }
                }
                return stringEncrypt;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
    [DataContract]
    public class ResponseData
    {
        [DataMember(Name = "iv")]
        public string stringIV { get; set; }

        [DataMember(Name = "salt")]
        public string stringSalt { get; set; }

        [DataMember(Name = "data")]
        public string Data { get; set; }
    }

    [DataContract]
    public class Info
    {
        [DataMember(Name = "Information")]
        public string stringInformation { get; set; }

    }
    [DataContract]
    public class Person
    {
        [DataMember(Name="Name")]
        public string stringName { get; set; }

        [DataMember(Name = "Gender")]
        public string stringGender { get; set; }

        [DataMember(Name = "Age")]
        public string stringAge { get; set; }

    }
}
