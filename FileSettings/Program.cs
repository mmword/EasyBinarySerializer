using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinarySerializer;
using System.IO;

namespace FileSettings
{
    class Program
    {
        static Random rnd = new Random();

        class MyClass
        {
            public MyClass()
            {

            }
            public int[] qarr;
            public float cb;
            public string name;
            public static MyClass RandomInstance
            {
                get
                {
                    MyClass m = new MyClass();
                    m.qarr = new int[] { 99, 44, 55 };
                    m.cb = ((float)rnd.NextDouble() * 100F);
                    m.name = (DateTime.Now.ToShortDateString()).ToString();
                    return m;
                }
            }
        }

        class HeavyClass
        {
            public HeavyClass()
            {
            }
            public void SetMSI()
            {
                msi = rnd.NextDouble() * 1000;
            }
           // public ResX resx;
            public int v;
            public int[] varr;
            public int[][] mvarr;
            public MyClass sa;
            public MyClass[] msa;
            public List<MyClass> lmsa;
            public string superName;
            public float fvalue;
            private double msi;
        }

        static HeavyClass Fill()
        {
            HeavyClass hc = new HeavyClass();
            hc.v = 77;
            hc.varr = new int[rnd.Next(1, 10)];
            for (int i = 0; i < hc.varr.Length; ++i)
                hc.varr[i] = rnd.Next(1, 100);
            hc.mvarr = new int[rnd.Next(1, 10)][];
            for (int i = 0; i < hc.mvarr.Length; ++i)
            {
                hc.mvarr[i] = new int[rnd.Next(0, 6)];
                for (int j = 0; j < hc.mvarr[i].Length; ++j)
                {
                    hc.mvarr[i][j] = rnd.Next(0, 150);
                }
            }
            hc.sa = MyClass.RandomInstance;
            hc.msa = new MyClass[rnd.Next(5, 10)];
            hc.lmsa = new List<MyClass>();
            for (int i = 0; i < hc.msa.Length; ++i)
                hc.msa[i] = MyClass.RandomInstance;
            for (int i = 0; i < rnd.Next(1, hc.msa.Length); ++i)
                hc.lmsa.Add(hc.msa[rnd.Next(1, hc.msa.Length-1)]);
            hc.superName = "Fill";
            hc.fvalue = (float)rnd.NextDouble() * 50F;
            return hc;
        }

        public struct Vector2
        {
            public int x, y;
        }

        static void DoSave(string f)
        {
            iBinarySerializer<int> ssave = Factory.QuerySerializerInterface<int>();
            //PropagateTypes(ssave);
            var data = Fill();
            var data2 = Fill();
            data2.SetMSI();
          //  Res r = new Res() { x = 44, y = 77, z = 11 };
            ssave.Declare(55, data, data);
            ssave.Declare(56, data2, data2);
          //  ssave.Declare(57, r, r);
            ssave.Save(f,SerializeFlags.None, SerializeFlags.CRC32 | SerializeFlags.SerializePrivateFields);
        }

        static void DoLoad(string f)
        {
            iBinarySerializer<int> load = Factory.QuerySerializerInterface<int>();
            load.OnTypeResolve += Load_OnTypeResolve;

          //  PropagateTypes(load);
            if (load.Load(f, SerializeFlags.ValidateKeyType, SerializeFlags.CRC32 | SerializeFlags.SerializePrivateFields) == LoadStatus.OK)
            {
                iSerializePrinter printer = Factory.QuerySerializerPrinterInterace();
                foreach (var _entry in load)
                    Console.WriteLine(printer.PrintToString(_entry.Value, _entry.Key.ToString()));
            }
            else
                Console.WriteLine("loading error");
        }

        private static Type Load_OnTypeResolve(DefinedType arg)
        {
            //return typeof(ResX);
            return null;
        }

        static void Main(string[] args)
        {

           // DoSave(@"F:\test.cfg");
            DoLoad(@"F:\test.cfg");

            Console.ReadLine();

        }
    }
}
