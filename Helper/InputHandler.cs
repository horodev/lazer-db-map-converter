﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LazerDBMapConverter
{
    public class InputHandler
    {
        private string[] args;

        public InputHandler(string[] args)
        {
            Console.WriteLine("osu!lazer db map converter");
            if (args.Length == 0)
            {
                Console.WriteLine("usage: lazer-db-converter 'path to osu folder (leave empty for standard path)' [-osz -dir]");
                Console.ReadKey();
                return;
            }
            if(args.Length == 1)
            {
                if(args[0] == "-osz")
                {
                    MainHandler.Convert(ConversionType.Osz);
                }
                else if(args[0] == "-dir")
                {
                    MainHandler.Convert(ConversionType.Directory);
                }
                else
                {
                    Console.WriteLine("invalid paramaters");
                }
            }
        }
    }
}
