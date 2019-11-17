﻿using System;
using System.Collections.Generic;
using System.IO;
using DataModel.Input;

namespace ImportModule
{
    public class Program
    {
        public static void ProcessData()
        {
            //            DataLoader dataLoader = new DataLoader();
            //            dataLoader.LoadCSV("Lab7_bus.csv");
            //            dataLoader.LoadXML("sample.xml");
            //            dataLoader.LoadUTX("sample.utx");
            //            dataLoader.setMinAndMaxCriterionValues();

            //            Console.WriteLine("Criteria:");
            //            for (int i = 0; i < dataLoader.CriterionList.Count; i++)
            //            {
            //                Console.WriteLine(dataLoader.CriterionList[i].Name);
            //            }
            //            Console.WriteLine();
            //
            //            Console.WriteLine("Alternatives:");
            //            for (int i = 0; i < dataLoader.AlternativeList.Count; i++)
            //            {
            //                Dictionary<string, float> dictionary = dataLoader.AlternativeList[i].CriteriaValues;
            //                foreach (KeyValuePair<string, float> kvp in dictionary)
            //                {
            //                    Console.WriteLine("{0} = {1}", kvp.Key, kvp.Value);
            //                }
            //                Console.WriteLine();
            //            }
            //            Console.ReadKey();
            

            XMCDALoader xmcdaLoader = new XMCDALoader();
            xmcdaLoader.LoadXMCDA(Path.Combine(Environment.CurrentDirectory, "xmcda"));

            Console.WriteLine("Criteria:");
            for (int i = 0; i < xmcdaLoader.CriterionList.Count; i++)
            {
                Console.WriteLine(xmcdaLoader.CriterionList[i].Name);
                Console.WriteLine(xmcdaLoader.CriterionList[i].CriterionDirection);
                Console.WriteLine();
            }

            Console.WriteLine("Alternatives:");
            for (int i = 0; i < xmcdaLoader.AlternativeList.Count; i++)
            {
                Dictionary<string, float> dictionary = xmcdaLoader.AlternativeList[i].CriteriaValues;
                foreach (KeyValuePair<string, float> kvp in dictionary)
                {
                    Console.WriteLine("{0} = {1}", kvp.Key, kvp.Value);
                }
                Console.WriteLine();
            }

            Console.ReadKey();
        }

    }
}