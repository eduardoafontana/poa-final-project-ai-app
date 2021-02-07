using System;

namespace TD3
{
    public class Program
    {
        public static void Main(string[] args)
        {
            int niveau = 1;
            bool continuer = true;
            int score_global = 0;

            while(continuer){
                Partie partie = new Partie(niveau);
                score_global += partie.Jouer();

                Console.WriteLine("le score actuel à la fin du niveau " + niveau + " est de " + score_global);
                Console.Write("continuer ? (Y/N) : ");
                
                string r = Console.ReadLine();
                if(r == "N" || r == "n"){
                    continuer = false;
                }
                
                niveau++;
            }
        }
    }
}
