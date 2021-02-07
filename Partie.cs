using System;

namespace TD3
{
    public class Partie
    {
        private int score;
        private int niveau;
        private Foret foret_magique;
        private Joueur joueur;

        public Partie(int niveau){
            score = 0;
            this.niveau = niveau;
            foret_magique = new Foret("foret magique", 2 + niveau);
            joueur = new Joueur("Bob", 2 + niveau);
        }

        public int Jouer(){
            Console.WriteLine(foret_magique);

            bool partie_en_cours = true;

            do{
                joueur.Placer(foret_magique.Spawn_l, foret_magique.Spawn_c);
                bool joueur_en_vie = true;

                Console.WriteLine(joueur.Name + " est apparu en case [" + joueur.Pos_l + "," + joueur.Pos_c + "]");
                do{
                    partie_en_cours = !joueur.Jouer(foret_magique);
                    joueur_en_vie = Etat_Joueur();
                }while(joueur_en_vie && partie_en_cours);
                
                if(joueur_en_vie == false){
                    Console.WriteLine(joueur.Name + " est mort");
                    joueur.Observer_et_Memoriser(foret_magique.Grille);
                    joueur.Score -= (niveau + 2) * (niveau + 2) * 10; 
                }
            }while(partie_en_cours);
            joueur.Score += (niveau + 2) * (niveau + 2) * 10; 

            return joueur.Score;
        }

        public bool Etat_Joueur(){
            string type_case_j = foret_magique.Grille[joueur.Pos_l, joueur.Pos_c].Type;
            if(type_case_j == "monstre" || type_case_j == "crevasse"){
                return false;
            }
            return true;
        }
    }
}