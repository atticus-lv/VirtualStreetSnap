/*
Code from https://github.com/sakya/AvaloniaLocalizationExample

MIT License

Copyright (c) 2022 Paolo Iommarini

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/

using Avalonia.Platform;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace VirtualStreetSnap.Localizer;

public class Localizer : INotifyPropertyChanged
{
    private const string IndexerName = "Item";
    private const string IndexerArrayName = "Item[]";
    private Dictionary<string, string> m_Strings = null;

    public bool LoadLanguage(string language)
    {
        Language = language;

        Uri uri = new Uri($"avares://VirtualStreetSnap/Assets/i18n/{language}.json");
        if (!AssetLoader.Exists(uri)) return false;
        using (StreamReader sr = new StreamReader(AssetLoader.Open(uri), Encoding.UTF8))
        {
            m_Strings = JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadToEnd());
        }

        Invalidate();

        return true;

    }

    public string Language { get; private set; }

    public string this[string key]
    {
        get
        {
            if (m_Strings != null && m_Strings.TryGetValue(key, out string res))
                return res.Replace("\\n", "\n");

            return $"{Language}:{key}";
        }
    }

    public static Localizer Instance { get; set; } = new Localizer();
    public event PropertyChangedEventHandler PropertyChanged;

    public void Invalidate()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
    }
    
}