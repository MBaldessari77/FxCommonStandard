using System;
using System.IO;
using System.Security.Cryptography;

namespace FxCommonStandard.Services
{
	/// <summary>
	/// Chiper service helper on AES256 .NET chiper.
	/// </summary>
	/// <remarks>
	/// This class generate a random IV every encripting and serialize it in encrypted result strongly enforce encripted data and it's privacy.
	/// </remarks>
	/// <see ref="https://msdn.microsoft.com/en-US/library/system.security.cryptography.aescryptoserviceprovider(v=vs.90).aspx"/>
	/// <see ref="http://stackoverflow.com/questions/8041451/good-aes-initialization-vector-practice"/>
	public class Aes256ChiperService
	{
		const int KeyLength = 256 / 8;  //32 byte

		public string EncriptBase64(Guid key1, Guid key2, string input)
		{
			//Generate a different IV for each calls
			using (var stream = new MemoryStream())
			using (var aes = Aes.Create())
			{
				byte[] iv = aes.IV;
				stream.Write(BitConverter.GetBytes(iv.Length), 0, sizeof(int));
				stream.Write(iv, 0, iv.Length);
				var key = new byte[KeyLength];
				Buffer.BlockCopy(key1.ToByteArray(), 0, key, 0, KeyLength / 2);
				Buffer.BlockCopy(key2.ToByteArray(), 0, key, KeyLength / 2, KeyLength / 2);
				byte[] encripted = EncryptStringToBytesAes(input, key, iv);
				stream.Write(encripted, 0, encripted.Length);
				return Convert.ToBase64String(stream.ToArray());
			}
		}

		public string DecriptBase64(Guid key1, Guid key2, string input)
		{
			byte[] buffer = Convert.FromBase64String(input);
			using (var stream = new MemoryStream(buffer))
			{
				var ivlengthbuffer = new byte[sizeof(int)];
				stream.Read(ivlengthbuffer, 0, sizeof(int));
				int ivlength = BitConverter.ToInt32(ivlengthbuffer, 0);
				var iv = new byte[ivlength];
				stream.Read(iv, 0, ivlength);
				var key = new byte[KeyLength];
				Buffer.BlockCopy(key1.ToByteArray(), 0, key, 0, KeyLength / 2);
				Buffer.BlockCopy(key2.ToByteArray(), 0, key, KeyLength / 2, KeyLength / 2);
				var encripted = new byte[buffer.Length - sizeof(int) - ivlength];
				stream.Read(encripted, 0, buffer.Length - sizeof(int) - ivlength);
				return DecryptStringFromBytesAes(encripted, key, iv);
			}
		}

		static byte[] EncryptStringToBytesAes(string plainText, byte[] key, byte[] iv)
		{
			// Check arguments.
			if (plainText == null || plainText.Length <= 0)
				throw new ArgumentNullException(nameof(plainText));
			if (key == null || key.Length <= 0)
				throw new ArgumentNullException(nameof(key));
			if (iv == null || iv.Length <= 0)
				throw new ArgumentNullException(nameof(iv));
			byte[] encrypted;
			// Create an AesCryptoServiceProvider object
			// with the specified key and IV.
			using (var aes = Aes.Create())
			{
				aes.Key = key;
				aes.IV = iv;

				// Create a decrytor to perform the stream transform.
				ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

				// Create the streams used for encryption.
				using (var msEncrypt = new MemoryStream())
				using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
				{
					using (var swEncrypt = new StreamWriter(csEncrypt))
						//Write all data to the stream.
						swEncrypt.Write(plainText);
					encrypted = msEncrypt.ToArray();
				}
			}

			// Return the encrypted bytes from the memory stream.
			return encrypted;
		}

		static string DecryptStringFromBytesAes(byte[] cipherText, byte[] key, byte[] iv)
		{
			// Check arguments.
			if (cipherText == null || cipherText.Length <= 0)
				throw new ArgumentNullException(nameof(cipherText));
			if (key == null || key.Length <= 0)
				throw new ArgumentNullException(nameof(key));
			if (iv == null || iv.Length <= 0)
				throw new ArgumentNullException(nameof(iv));

			// Declare the string used to hold
			// the decrypted text.
			string plaintext;

			// Create an AesCryptoServiceProvider object
			// with the specified key and IV.
			using (var aes = Aes.Create())
			{
				aes.Key = key;
				aes.IV = iv;

				// Create a decrytor to perform the stream transform.
				ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

				// Create the streams used for decryption.
				using (var msDecrypt = new MemoryStream(cipherText))
				using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
				using (var srDecrypt = new StreamReader(csDecrypt))
					// Read the decrypted bytes from the decrypting stream
					// and place them in a string.
					plaintext = srDecrypt.ReadToEnd();
			}

			return plaintext;
		}
	}
}
