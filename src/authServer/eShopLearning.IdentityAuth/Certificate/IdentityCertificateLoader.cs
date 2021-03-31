using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace eShopLearning.IdentityAuth.Certificate
{
    /// <summary>
    /// 加载 Identity Server 4 用的证书
    /// </summary>
    public class IdentityCertificateLoader
    {
        /// <summary>
        /// 获取 用于 id4 的token签名私钥证书
        /// </summary>
        /// <returns></returns>
        public static X509Certificate2 Get()
        {
            var assembly = typeof(IdentityCertificateLoader).GetTypeInfo().Assembly;
            var names = assembly.GetManifestResourceNames();

            /***********************************************************************************************
             *  Please note that here we are using a local certificate only for testing purposes. In a 
             *  real environment the certificate should be created and stored in a secure way, which is out
             *  of the scope of this project.
             **********************************************************************************************/
            using (var stream = assembly.GetManifestResourceStream("eShopLearning.IdentityAuth.Certificate.id4test.pfx"))
            {
                return new X509Certificate2(ReadStream(stream), "123456"); // 123456 指的是密码
            }
        }

        /// <summary>
        /// 读取证书，并转换为字节数组
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static byte[] ReadStream(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
