using System;
using System.Collections.Generic;
using System.Text;

namespace Palanteer
{
    public static class UoAutomapImporter
    {
        public static Place[] Import(string import)
        {
            List<Place> places = new List<Place>();

            string[] lines = import.Split(new string[] {"\r\n", "\n"}, StringSplitOptions.None);

            foreach (var line in lines)
            {
                if (line.Length == 0 || line[0] != '+' && line[0] != '-')
                    continue;

                int importPosition = 1;
                while (line[importPosition] != ':')
                    importPosition++;

                string type = line.Substring(1, importPosition - 1);
                importPosition += 2;

                int start = importPosition;
                while (char.IsNumber(line[importPosition]))
                    importPosition++;

                string xstring = line.Substring(start, importPosition - start);
                int x = int.Parse(xstring);

                importPosition++;
                start = importPosition;
                while (char.IsNumber(line[importPosition]))
                    importPosition++;

                string ystring = line.Substring(start, importPosition - start);
                int y = int.Parse(ystring);

                importPosition += 3;
                start = importPosition;
                while (importPosition < line.Length)
                    importPosition++;

                string name = line.Substring(start, importPosition - start).TrimEnd('\r', '\n');

                places.Add(
                    new Place()
                    {
                        Name = name,
                        Type = type,
                        X = x,
                        Y = y,
                    });
            }

            return places.ToArray();
        }
    }
}
