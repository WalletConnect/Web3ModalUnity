#!/usr/bin/env dotnet-script

#r "nuget: QRCoder"

using QRCoder;
using System;
using System.IO;

if (args.Length != 2)
{
    Console.WriteLine("Usage: dotnet-script generate-qr.csx <url> <output-file>");
    return;
}

string url = args[0];
string outputFile = args[1];

QRCodeGenerator qrGenerator = new QRCodeGenerator();
QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
byte[] qrCodeImage = qrCode.GetGraphic(20);

File.WriteAllBytes(outputFile, qrCodeImage);
Console.WriteLine($"QR Code saved to {outputFile}");
