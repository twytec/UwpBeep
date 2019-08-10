using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace UwpBeep
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(tbAmplitude.Text, out int a) && int.TryParse(tbFrequency.Text, out int f) && int.TryParse(tbDuration.Text, out int d))
            {
                var beep = await BeepBeep(a, f, d);
                mp.SetSource(beep, string.Empty);
                mp.Play();
            }
        }

        public async Task<IRandomAccessStream> BeepBeep(int Amplitude, int Frequency, int Duration)
        {
            double A = ((Amplitude * (Math.Pow(2, 15))) / 1000) - 1;
            double DeltaFT = 2 * Math.PI * Frequency / 44100.0;

            int Samples = 441 * Duration / 10;
            int Bytes = Samples * 4;
            int[] Hdr = { 0X46464952, 36 + Bytes, 0X45564157, 0X20746D66, 16, 0X20001, 44100, 176400, 0X100004, 0X61746164, Bytes };

            InMemoryRandomAccessStream ims = new InMemoryRandomAccessStream();
            IOutputStream outStream = ims.GetOutputStreamAt(0);
            DataWriter dw = new DataWriter(outStream);
            dw.ByteOrder = ByteOrder.LittleEndian;

            for (int I = 0; I < Hdr.Length; I++)
            {
                dw.WriteInt32(Hdr[I]);
            }
            for (int T = 0; T < Samples; T++)
            {
                short Sample = System.Convert.ToInt16(A * Math.Sin(DeltaFT * T));
                dw.WriteInt16(Sample);
                dw.WriteInt16(Sample);
            }
            await dw.StoreAsync();
            await outStream.FlushAsync();
            return ims;
        }
    }
}
