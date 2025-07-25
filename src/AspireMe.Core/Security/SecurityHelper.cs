using System.Security.Cryptography;
using System.Text;

namespace AspireMe.Core.Security;

public static class SecurityHelper
{
    private static readonly string _encryptionKey = "mysecretkey12345";
    public static string EncryptValue( string value )
    {
        if ( string.IsNullOrEmpty( value ) )
            return value;


        using var aesAlg = Aes.Create();
        aesAlg.Key = Encoding.UTF8.GetBytes( _encryptionKey );  // 16 bytes key for AES-128

        aesAlg.GenerateIV();
        var iv = aesAlg.IV;

        using var encryptor = aesAlg.CreateEncryptor( aesAlg.Key, iv );
        var valueBytes = Encoding.UTF8.GetBytes( value );
        var encryptedBytes = encryptor.TransformFinalBlock( valueBytes, 0, valueBytes.Length );

        // Convert the encrypted byte array to a Base64 string
        var encryptedString = Convert.ToBase64String( encryptedBytes );

        var ivBase64 = Convert.ToBase64String( iv );
        return ivBase64 + ":" + encryptedString;  // Store IV and encrypted value together
    }

    // Method to decrypt the property value using AES
    public static string DecryptValue( string encryptedValue )
    {
        if ( string.IsNullOrEmpty( encryptedValue ) )
            return encryptedValue;

        // Split the stored value into IV and encrypted data
        var parts = encryptedValue.Split( ':' );
        if ( parts.Length != 2 )
            return encryptedValue;

        var ivBase64 = parts[0];
        var encryptedBase64 = parts[1];

        var iv = Convert.FromBase64String( ivBase64 );
        var encryptedBytes = Convert.FromBase64String( encryptedBase64 );

        using var aesAlg = Aes.Create();
        aesAlg.Key = Encoding.UTF8.GetBytes( _encryptionKey );  // 16 bytes key for AES-128

        using var decryptor = aesAlg.CreateDecryptor( aesAlg.Key, iv );
        var decryptedBytes = decryptor.TransformFinalBlock( encryptedBytes, 0, encryptedBytes.Length );

        // Convert decrypted byte array back to string
        return Encoding.UTF8.GetString( decryptedBytes );
    }
}
