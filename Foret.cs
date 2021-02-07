using System;

namespace TD3
{
    public class Foret 
    {
        private string name;
        private int dim;
        private Case[,] grille;
        private int nb_monstre;
        private int nb_crevasse;
        private int[] spawn;

        //genere un foret donc toutes les cases sans crevasses sont accessibles a n'importe quelle autre casse sasn crevasse
        public Foret(string name, int dim) 
        {
            this.name = name;
            this.dim = dim;
            grille = new Case[dim, dim];
            nb_monstre = (int) Math.Truncate(0.2 * dim * dim); //probabilite d'apparition de montre
            nb_crevasse = (int) Math.Truncate(0.15 * dim * dim); //probabilite d'apparition de crevasse
            
            bool test_crevasses = false;
            
            Random random = new Random();

            do{
                //initialiser cases
                for(int l = 0; l < grille.GetLength(0); l++){
                    for(int c = 0; c < grille.GetLength(1); c++){
                        string name_case = name + ":" + l.ToString() + "_" + c.ToString();
                        grille[l,c] = new Case(name_case, "vide");
                    }
                }

                //placement des crevasses
                int nb_crevasse_placee = 0;

                while(nb_crevasse_placee < nb_crevasse){
                    int l = random.Next(0, grille.GetLength(0));
                    int c = random.Next(0, grille.GetLength(1));
                    if(grille[l,c].Type == "vide"){
                        grille[l,c].Type = "crevasse";
                        nb_crevasse_placee++;
                    }
                }

                //tester acces
                    //generer une grille nxn remplie de false
                bool[,] grille_test = new bool[dim, dim];
                for(int i=0; i < grille_test.GetLength(0); i++){
                    for(int j=0; j < grille_test.GetLength(1); j++){
                        grille_test[i,j] = false;
                    }
                }

                    //trouver une case vide
                int[] coo = new int[] {0, 0};
                for(int l = 0; l < grille.GetLength(0); l++){
                    for(int c = 0; c < grille.GetLength(1); c++){
                        if(grille[l,c].Type == "vide"){
                            coo[0] = l;
                            coo[1] = c;
                        }
                    }
                }

                    //diffusion de la valeur true à partir de la case vide
                diffusion(coo, grille_test);
                
                test_crevasses = true;

                    //verifier si toute les cases vides sont true
                for(int i=0; i < grille_test.GetLength(0); i++){
                    for(int j=0; j < grille_test.GetLength(1); j++){
                        if(grille_test[i,j] == false){
                            if(grille[i, j].Type == "vide"){
                                test_crevasses = false;
                            }
                        }
                    }
                }

            }while(test_crevasses == false);

            //placer monstres
            int nb_monstre_place = 0;

            while(nb_monstre_place < nb_monstre){
                int l = random.Next(0, grille.GetLength(0));
                int c = random.Next(0, grille.GetLength(1));
                if(grille[l,c].Type == "vide"){
                    grille[l,c].Type = "monstre";
                    nb_monstre_place++;
                }
            }

            //placer portail
            int nb_portail_place = 0;

            while(nb_portail_place < 1){
                int l = random.Next(0, grille.GetLength(0));
                int c = random.Next(0, grille.GetLength(1));
                if(grille[l,c].Type == "vide"){
                    grille[l,c].Type = "portail";
                    nb_portail_place++;
                }
            }

            //update cases
            for(int l = 0; l < grille.GetLength(0); l++){
                for(int c = 0; c < grille.GetLength(1); c++){
                    int[] coo = {l, c};
                    Update(coo);
                }
            }

            //placer le point d'apparition du joueur
            bool[] case_vide = new bool[dim * dim];
            int nb_case_vide = 0;
            for(int l = 0; l < grille.GetLength(0); l++){
                for(int c = 0; c < grille.GetLength(1); c++){
                    if(grille[l,c].Type == "vide"){
                        case_vide[l*dim + c] = true;
                        nb_case_vide ++;
                    }
                    else{
                        case_vide[l*dim + c] = false;
                    }
                }
            }

            int case_vide_choisie = random.Next(0, nb_case_vide - 1);

            nb_case_vide = 0;
            for(int i = 0; i < case_vide.GetLength(0); i++){
                if(case_vide[i] == true){
                    if(nb_case_vide == case_vide_choisie){
                        spawn = new int[] {(int) i / dim , i % dim};
                    }
                    nb_case_vide ++;
                }
            }

        }


        //update la case si un joueur lance une roche
        public void Utilisation_de_roches(int l, int c) 
        {
            int[] coo = {l, c};
            if(0 <= coo[0] && coo[0] < dim && 0 <= coo[1] && coo[1] < dim){
                if(grille[coo[0],coo[1]].Type == "monstre"){
                    grille[coo[0],coo[1]].Type = "vide";
                }
                int[] cooN = {coo[0]+1, coo[1]};
                Update(cooN);
                int[] cooS = {coo[0]-1, coo[1]};
                Update(cooS);
                int[] cooW = {coo[0], coo[1]-1};
                Update(cooW);
                int[] cooE = {coo[0], coo[1]+1};
                Update(cooE);
            }
        }


        //utilise dans la generation des falaises pour voir si chaque case est accessible a une autre
        private void diffusion(int[] coo, bool[,] grille_test) 
        {
            if(grille[coo[0], coo[1]].Type == "vide"){
                grille_test[coo[0], coo[1]] = true;

                for(int dl = -1; dl <= 1; dl+=1){
                    if(0 <= coo[0] + dl && coo[0] + dl < dim){
                        if(grille_test[coo[0] + dl, coo[1]] == false){
                            int[] new_coo = new int[] {coo[0] + dl, coo[1]};
                            diffusion(new_coo, grille_test);
                        }
                    }
                }
                for(int dc = -1; dc <= 1; dc+=1){
                    if(0 <= coo[1] + dc && coo[1] + dc < dim){
                        if(grille_test[coo[0], coo[1] + dc] == false){
                            int[] new_coo = new int[] {coo[0], coo[1] + dc};
                            diffusion(new_coo, grille_test);
                        }
                    }
                }
            }
        }


        //met a jour le vent, les odeurs et les zones lumineuses sur la case
        private void Update(int[] coo) 
        {
            int l = coo[0];
            int c = coo[1];

            if(0 <= l && l < dim && 0 <= c && c < dim){
                if(grille[l,c].Type == "portail"){
                    grille[l,c].Luminosite = "forte";
                }

                int[,] delta = {{-1, 0}, {0, 1}, {1, 0}, {0, -1}};
                for(int i = 0; i < delta.GetLength(0); i++){
                    if(0 <= l + delta[i,0] && l + delta[i,0] < dim && 0 <= c + delta[i,1] && c + delta[i,1] < dim){
                        if(grille[l + delta[i,0], c + delta[i,1]].Type == "monstre"){
                            grille[l,c].Odeur = "mauvaise";
                        }
                        if(grille[l + delta[i,0], c + delta[i,1]].Type == "crevasse"){
                            grille[l,c].Vitesse_vent = "fort";
                        }
                    }
                }
            }
        }


        public override string ToString()
        {
            string r = "\n\n" + name + "\n. : vide\nv : crevasse\nM : monstre\nO : portail\n\n";
            for(int l = 0; l < grille.GetLength(0); l++){
                for(int c = 0; c < grille.GetLength(1); c++){
                    r += grille[l,c].ToString();
                }
                r += "\n";
            }
            return r;
        }
        

        public int Spawn_l{
            get{return spawn[0];}
        }


        public int Spawn_c{
            get{return spawn[1];}
        }


        public Case[,] Grille{
            get{return grille;}
        }
    }
}
