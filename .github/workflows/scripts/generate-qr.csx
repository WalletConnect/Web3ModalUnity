#!/usr/bin/env dotnet-script

#r "nuget: ZXing.Net, 0.16.9"
#r "nuget: ZXing.Net.Bindings.SkiaSharp, 0.16.9"
#r "nuget: SkiaSharp, 2.88.3"

using System;
using System.IO;
using ZXing;
using ZXing.QrCode;
using ZXing.SkiaSharp;
using ZXing.SkiaSharp.Rendering;
using SkiaSharp;

if (Args.Count != 2)
{
    Console.WriteLine("Usage: dotnet-script generate-qr.csx <url> <output-file>");
    return;
}

string url = Args[0];
string outputFile = Args[1];

var qrCodeEncodingOptions = new QrCodeEncodingOptions
{
    Height = 512,
    Width = 512,
    Margin = 4
};

var barcodeWriter = new BarcodeWriter<SKBitmap>
{
    Format = BarcodeFormat.QR_CODE,
    Options = qrCodeEncodingOptions,
    Renderer = new SKBitmapRenderer()
};

using (var bitmap = barcodeWriter.Write(url))
{
    using (var image = SKImage.FromBitmap(bitmap))
    using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
    using (var stream = File.OpenWrite(outputFile))
    {
        data.SaveTo(stream);
    }
}

Console.WriteLine($"QR Code saved to {outputFile}");
