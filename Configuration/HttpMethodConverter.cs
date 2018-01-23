using System;
using System.Net.Http;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;

namespace DynamicPowerShellApi.Configuration
{
    //
    // Résumé :
    //     Effectue la conversion entre les valeurs de chaîne et de type. Cette classe ne
    //     peut pas être héritée.
    public sealed class HttpMethodConverter : TypeConverter
    {
        //
        // Résumé :
        //     Initialise une nouvelle instance de la classe System.Configuration.TypeNameConverter.
        public HttpMethodConverter()
        {

        }

        //
        // Résumé :
        //     Convertit un objet System.String en un objet System.Type.
        //
        // Paramètres :
        //   ctx:
        //     Le System.ComponentModel.ITypeDescriptorContext objet utilisé pour les conversions
        //     de type.
        //
        //   ci:
        //     Le System.Globalization.CultureInfo objet utilisé pendant la conversion.
        //
        //   data:
        //     Le System.String objet à convertir.
        //
        // Retourne :
        //     Le System.Type qui représente le data paramètre.
        //
        // Exceptions :
        //   T:System.ArgumentException:
        //     Le System.Type ne peut pas être résolu.
        public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data)
        {
            return new HttpMethod(data.ToString());
        }
        //
        // Résumé :
        //     Convertit un objet System.Type en un objet System.String.
        //
        // Paramètres :
        //   ctx:
        //     Le System.ComponentModel.ITypeDescriptorContext objet utilisé pour les conversions
        //     de type.
        //
        //   ci:
        //     Le System.Globalization.CultureInfo objet utilisé pendant la conversion.
        //
        //   value:
        //     Valeur à convertir.
        //
        //   type:
        //     Type vers lequel effectuer la conversion.
        //
        // Retourne :
        //     Le System.String qui représente le value paramètre.
        public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type)
        {
            return value.ToString();
        }
    }
}
