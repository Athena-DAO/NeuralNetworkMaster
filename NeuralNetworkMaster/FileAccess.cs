using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NeuralNetworkMaster
{
    class FileAccess
    {

        public int Lines { get; }
        private string FileName;
        private Object fileLock = new Object();
        private int nextLine;
        private Dictionary<int, int> dict = new Dictionary<int, int>();
        private StreamReader stream;
        public FileAccess(String FileName)
        {
            this.FileName = FileName;
            nextLine = 0;

            StreamReader stream = new StreamReader(FileName);
            Lines = 0;
            while (!stream.EndOfStream)
            {
                stream.ReadLine();
                Lines++;
            }
            stream.Close();
        }

        public void OpenStream()
        {
            stream = new StreamReader(FileName);
        }

        public void CloseStream()
        {
            stream.Close();
        }

        public string ReadNextLines(int numberOfLines)
        {
            lock (fileLock)
            {

                stream = new StreamReader(FileName);
                StringBuilder stringBuilder = new StringBuilder(numberOfLines);
                for (int j = 0; j < numberOfLines - 1; j++)
                {
                    stringBuilder.AppendLine(stream.ReadLine());
                }
                stringBuilder.Append(stream.ReadLine());
                nextLine += numberOfLines;
                return stringBuilder.ToString();
            }

        }

        public string ReadFile()
        {
            lock (fileLock)
            {

                stream = new StreamReader(FileName);
                StringBuilder stringBuilder = new StringBuilder(Lines);
                for (int j = 0; j < Lines - 1; j++)
                {
                    stringBuilder.AppendLine(stream.ReadLine());
                }
                stringBuilder.Append(stream.ReadLine());
                nextLine += Lines;
                return stringBuilder.ToString();
            }

        }

    }
}
