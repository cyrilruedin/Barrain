using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnserializeScore
{
    class Program
    {
        static void Main(string[] args)
        {
            PlayerBarrageStatistics p = PlayerBarrageStatistics.LoadBarrageData();
            int nID = p.GetNSequenceID();
            for(int i = 0; i < nID; i++)
            {

            }

            using (System.IO.TextWriter writer = File.CreateText("saveBarrage.csv"))
            {

                writer.WriteLine("Resultat");

                writer.Write("Sequence;");
                writer.Write("Nom du niveau ;");
                writer.Write("Reussite ;");
                writer.Write("Temps ;");
                writer.Write("Nombre de blocs manquant ;");
                writer.Write("Nombre de transport ;");
                writer.Write("Nombre de pieces ;");
                writer.Write("Taille moyenne des pieces ;");

                writer.Write(writer.NewLine);

                for (int i = 0; i < nID; i++)
                {
                    writer.Write(i);
                    writer.Write(";");

                    writer.Write(p.GetLevelName(i));
                    writer.Write(";");

                    writer.Write(p.GetLevelSuccess(i));
                    writer.Write(";");

                    writer.Write(p.GetTime(i));
                    writer.Write(";");

                    writer.Write(p.GetNMissingBlocs(i));
                    writer.Write(";");

                    writer.Write(p.GetNTransport(i));
                    writer.Write(";");

                    writer.Write(p.GetNPiece(i));
                    writer.Write(";");

                    writer.Write(p.GetAveragePieceSize(i));
                    writer.Write(";");
                    writer.Write(writer.NewLine);
                }

            }
        }
    }
}


