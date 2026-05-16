using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SecureWebApp.Data
{
    public class EncryptionConverter : ValueConverter<string, string>
    {
        public EncryptionConverter(IDataProtector protector, ConverterMappingHints? mappingHints = null)
            : base(
                v => protector.Protect(v),
                v => protector.Unprotect(v),
                mappingHints)
        {
        }
    }
}
