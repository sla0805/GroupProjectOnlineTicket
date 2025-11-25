






using QRCoder;
using System;
using static QRCoder.QRCodeGenerator;

public static class QrHelper
{
    public static string GenerateQrBase64(string text)
    {
        using var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(text, ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        var qrBytes = qrCode.GetGraphic(20);
        return $"data:image/png;base64,{Convert.ToBase64String(qrBytes)}";
    }
}
