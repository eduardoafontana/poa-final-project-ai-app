using System;
using System.Linq;

namespace TD3
{
    public class Joueur
    {
        private string name;
        private int pos_l;
        private int pos_c;
        private float[,,] memoire;
        private int dim_foret;
        int score = 0;

        public Joueur(string name, int dim_foret)
        {
            this.name = name;
            this.dim_foret = dim_foret;
            memoire = new float[dim_foret,dim_foret,7];
            for(int l = 0; l < dim_foret; l++){
                for(int c = 0; c < dim_foret; c++){
                    memoire[l,c,0] = -1; //proba montre
                    memoire[l,c,1] = -1; //proba crevasse
                    memoire[l,c,2] = -1; //proba portail
                    memoire[l,c,3] = 0;  //nb passage
                    memoire[l,c,4] = -1; //odeur
                    memoire[l,c,5] = -1; //vent
                    memoire[l,c,6] = -1; //luminosite
                }
            }
        }


        public bool Jouer(Foret foret){
            Observer_et_Memoriser(foret.Grille);
            Analyse();
            char d = Reflexion();
            
            if(d == 'P'){
                Console.WriteLine(name + " prend le portail et passe au niveau suivant.");
            }
            else{
                Console.WriteLine(name + " va vers " + d);
            }
           
            return Bouger_vers(d, foret);
        }

        //observe et memorise l'ensemble des capteurs de la case
        public void Observer_et_Memoriser(Case[,] foret){
            if(foret[pos_l,pos_c].Type == "monstre"){
                memoire[pos_l,pos_c,0] = 100;
            }
            else{
                memoire[pos_l,pos_c,0] = 0;
            }
            if(foret[pos_l,pos_c].Type == "crevasse"){
                memoire[pos_l,pos_c,1] = 100;
            }
            else{
                memoire[pos_l,pos_c,1] = 0;
            }
            if(Sentir(foret) == "mauvaise"){
                memoire[pos_l,pos_c,4] = 1;
            }
            else{
                memoire[pos_l,pos_c,4] = 0;
            }
            if(Ressentir_vent(foret) == "fort"){
                memoire[pos_l,pos_c,5] = 1;
            }
            else{
                memoire[pos_l,pos_c,5] = 0;
            }
            if(Regarder_luminosite(foret) == "forte"){
                memoire[pos_l,pos_c,6] = 1;
            }
            else{
                memoire[pos_l,pos_c,6] = 0;
            }
        }


        //met a jour les connaissances du joueur de la foret
        //la memoire du joueur attribu a chaque case de la foret une liste de 7 valeurs : 
        //  3 pour la probabilité d'avoir soit un monstre, soit une crevasse, soit le portail 
        //  1 valeur pour le nombre de passage du joueur sur la case
        //  3 pour connaitre l'etat des capteurs sur la case (lumiere, ordeur, vent)
        public void Analyse(){
            for(int l = 0; l < memoire.GetLength(0); l++){
                for(int c = 0; c < memoire.GetLength(1); c++){
                    if(memoire[l,c,6] == -1){
                        memoire[l,c,2] = 50;
                    }
                    if(memoire[l,c,6] == 0){
                        memoire[l,c,2] = 0;
                    }
                    if(memoire[l,c,6] == 1){
                        memoire[l,c,2] = 100;
                    }
                    if(memoire[l,c,3] == 0){//la case n'a jamais ete exploree
                        bool montre_possible = true;
                        bool crevasse_possible = true;
                        float proba_montre = 4;// 4/8 <=> 50%
                        float proba_crevasse = 4;// 4/8 <=> 50%

                        // verification des cases N S W E
                        int[,] delta = {{-1, 0}, {0, 1}, {1, 0}, {0, -1}};
                        for(int i = 0; i < delta.GetLength(0); i++){
                            if(0 <= l + delta[i,0] && l + delta[i,0] < dim_foret && 0 <= c + delta[i,1] && c + delta[i,1] < dim_foret){
                                if(memoire[l + delta[i,0], c + delta[i,1],4] == 0){
                                    montre_possible = false;
                                }
                                else{
                                    if(memoire[l + delta[i,0], c + delta[i,1],4] == 1){
                                        proba_montre++;
                                    }
                                }
                                if(memoire[l + delta[i,0], c + delta[i,1],5] == 0){
                                    crevasse_possible = false;
                                }
                                else{
                                    if(memoire[l + delta[i,0], c + delta[i,1],5] == 1){
                                        proba_crevasse++;
                                    }
                                }
                            }
                        }
                        
                        if(montre_possible){
                            memoire[l,c,0] = 100*proba_montre/8;
                        }
                        else{
                            memoire[l,c,0] = 0;
                        }

                        if(crevasse_possible){
                            memoire[l,c,1] = 100*proba_crevasse/8;
                        }
                        else{
                            memoire[l,c,1] = 0;
                        }
                    }
                }
            }
        }


        //determiner la case la plus probable de contenir le portail
        public char Reflexion(){
            //parcours memoire pour trouver max proba portail
            float proba_portail_max = 0;
            for(int l = 0; l < memoire.GetLength(0); l++){
                for(int c = 0; c < memoire.GetLength(1); c++){
                    proba_portail_max = Math.Max(proba_portail_max, memoire[l,c,2]);
                }
            }

            //parcours memoire pour trouver min proba crevasse pour max proba portail
            float proba_crevasse_min = 100;
            for(int l = 0; l < memoire.GetLength(0); l++){
                for(int c = 0; c < memoire.GetLength(1); c++){
                    if(memoire[l,c,2] == proba_portail_max){
                        proba_crevasse_min = Math.Min(proba_crevasse_min, memoire[l,c,1]);
                    }
                }
            }

            //calculer eloignement de chaque case portail avec proba la plus forte
            int[,] eloignement = new int[dim_foret, dim_foret];
            for(int l = 0; l < eloignement.GetLength(0); l++){
                for(int c = 0; c < eloignement.GetLength(1); c++){
                    eloignement[l,c] = Nb_cases_vers(l, c);
                }
            }
            eloignement[pos_l, pos_c] = 0;
            
            //trouver la distance la plus faible avec proba portail la plus forte et min proba crevasse
            int case_la_plus_proche = Int32.MaxValue;
            for(int l = 0; l < memoire.GetLength(0); l++){
                for(int c = 0; c < memoire.GetLength(1); c++){
                    if(memoire[l,c,2] == proba_portail_max && memoire[l,c,1] == proba_crevasse_min){
                        case_la_plus_proche = Math.Min(case_la_plus_proche, eloignement[l,c]);
                    }
                }
            }

            //trouver une case
            int lf = -1;
            int cf = -1;
            for(int l = 0; l < memoire.GetLength(0); l++){
                for(int c = 0; c < memoire.GetLength(1); c++){
                    if(memoire[l,c,2] == proba_portail_max && memoire[l,c,1] == proba_crevasse_min && eloignement[l,c] == case_la_plus_proche){
                        lf = l;
                        cf = c;
                    }
                }
            }

            //si c'est notre case, prendre le portail
            if(pos_l == lf && pos_c == cf){
                return 'P';
            }

            //sinon se diriger vers la case
            return Direction_vers(lf, cf);
        }


        //bouge le joueur vers l'une des 4 directions, si la proba d'un monstre sur la case d'arrivee est non nul, le joueur jete une pierre
        public bool Bouger_vers(char d, Foret foret){
            score -= 1;
            if(d == 'N'){
                try{
                    if(memoire[pos_l - 1, pos_c, 0] > 0){
                        Jeter_pierre('N', foret);
                    }
                }
                catch{}
                Placer(pos_l - 1, pos_c);
                return false;
            }
            if(d == 'S'){
                try{
                    if(memoire[pos_l + 1, pos_c, 0] > 0){
                        Jeter_pierre('S', foret);
                    }
                }
                catch{}
                Placer(pos_l + 1, pos_c);
                return false;
            }
            if(d == 'W'){
                try{
                    if(memoire[pos_l, pos_c - 1, 0] > 0){
                        Jeter_pierre('W', foret);
                    }
                }
                catch{}
                Placer(pos_l, pos_c - 1);
                return false;
            }
            if(d == 'E'){
                try{
                    if(memoire[pos_l, pos_c + 1, 0] > 0){
                        Jeter_pierre('E', foret);
                    }
                }
                catch{}
                Placer(pos_l, pos_c + 1);
                return false;
            }
            if(d == 'P'){
                return Prendre_portail(foret.Grille);
            }
            Console.WriteLine("/!\\ " + d);
            Console.ReadLine();
            return false;
        }



        //place le joueur sur la grille
        public bool Placer(int l, int c){
            bool test = false;
            if(0 <= l && l < dim_foret){
                if(0 <= c && c < dim_foret){
                    pos_l = l;
                    pos_c = c;
                    memoire[pos_l,pos_c,3]++; //ajoute 1 au nombre de passage sur cette case dans la memoire du joueur
                    test = true;
                }
            }
            return test;
        }


        //renvoie le nombre de case le plus proche de l'objectif situé en [lf, cf]
        public int Nb_cases_vers(int lf, int cf){
            int n_N = Int32.MaxValue;
            int n_S = Int32.MaxValue;
            int n_W = Int32.MaxValue;
            int n_E = Int32.MaxValue;
            int l0 = pos_l;
            int c0 = pos_c;
            int[,] passage = new int[dim_foret, dim_foret];
            for(int i = 0; i < dim_foret; i++){
                for(int j = 0; j < dim_foret; j++){
                    passage[i,j] = Int32.MaxValue;
                }
            }
            passage[l0,c0] = 0;
            try{
                n_N = Direction_vers_recursif(lf, cf, l0 - 1, c0, passage, 1);
            }
            catch{
            }
            try{
                n_S = Direction_vers_recursif(lf, cf, l0 + 1, c0, passage, 1);
            }
            catch{
            }
            try{
                n_W = Direction_vers_recursif(lf, cf, l0, c0 - 1, passage, 1);
            }
            catch{
            }
            try{
                n_E = Direction_vers_recursif(lf, cf, l0, c0 + 1, passage, 1);
            }
            catch{
            }
            //Console.WriteLine(n_N + " " + n_S + " " + n_W + " " + n_E);
            int[] list_n = {n_N, n_S, n_W, n_E};
            return list_n.Min();
        }

        //renvoie la direction pour se rendre à l'objectif situé en [lf, cf]
        public char Direction_vers(int lf, int cf){
            int n_N = Int32.MaxValue;
            int n_S = Int32.MaxValue;
            int n_W = Int32.MaxValue;
            int n_E = Int32.MaxValue;
            int l0 = pos_l;
            int c0 = pos_c;
            int[,] passage = new int[dim_foret, dim_foret];
            for(int i = 0; i < dim_foret; i++){
                for(int j = 0; j < dim_foret; j++){
                    passage[i,j] = Int32.MaxValue;
                }
            }
            passage[l0,c0] = 0;
            try{
                if(memoire[l0 - 1, c0, 1] < 100){
                    n_N = Direction_vers_recursif(lf, cf, l0 - 1, c0, passage, 1);
                }
            }
            catch{
            }
            try{
                if(memoire[l0 + 1, c0, 1] < 100){
                    n_S = Direction_vers_recursif(lf, cf, l0 + 1, c0, passage, 1);
                }
            }
            catch{
            }
            try{
                if(memoire[l0, c0 - 1, 1] < 100){
                    n_W = Direction_vers_recursif(lf, cf, l0, c0 - 1, passage, 1);
                }
            }
            catch{
            }
            try{
                if(memoire[l0, c0 + 1, 1] < 100){
                    n_E = Direction_vers_recursif(lf, cf, l0, c0 + 1, passage, 1);
                }
            }
            catch{
            }
            //Console.WriteLine(n_N + " " + n_S + " " + n_W + " " + n_E);
            int[] list_n = {n_N, n_S, n_W, n_E};
            if(list_n.Min() == Int32.MaxValue){
                return 'X';
            }
            if(list_n.Min() == n_N){
                return 'N';
            }
            if(list_n.Min() == n_S){
                return 'S';
            }
            if(list_n.Min() == n_W){
                return 'W';
            }
            if(list_n.Min() == n_E){
                return 'E';
            }
            return 'X';
        }

        //permet de trouver un chemin evitant les crevasses (les montres ne sont pas pris en compte)
        public int Direction_vers_recursif(int lf, int cf, int l0, int c0, int[,] passage, int n){
            passage[l0,c0] = n;
            if(l0 == lf && c0 == cf){
                return n;
            }
            if((cf == c0 && lf == l0 - 1) || (cf == c0 && lf == l0 + 1) || (cf == c0 + 1 && lf == l0) || (cf == c0 - 1 && lf == l0)){
                return n + 1;
            }
            int n_N = Int32.MaxValue;
            int n_S = Int32.MaxValue;
            int n_W = Int32.MaxValue;
            int n_E = Int32.MaxValue;
            try{
                if(memoire[l0 - 1, c0, 1] < 100 && passage[l0 - 1,c0] > n + 1){
                    n_N = Direction_vers_recursif(lf, cf, l0 - 1, c0, passage, n + 1);
                }
            }
            catch{
            }
            try{
                if(memoire[l0 + 1, c0, 1] < 100 && passage[l0 + 1,c0] > n + 1){
                    n_S = Direction_vers_recursif(lf, cf, l0 + 1, c0, passage, n + 1);
                }
            }
            catch{
            }
            try{
                if(memoire[l0, c0 - 1, 1] < 100 && passage[l0,c0 - 1] > n + 1){
                    n_W = Direction_vers_recursif(lf, cf, l0, c0 - 1, passage, n + 1);
                }
            }
            catch{
            }
            try{
                if(memoire[l0, c0 + 1, 1] < 100 && passage[l0,c0 + 1] > n + 1){
                    n_E = Direction_vers_recursif(lf, cf, l0, c0 + 1, passage, n + 1);
                }
            }
            catch{
            }
            int[] list_n = {n_N, n_S, n_W, n_E};
            return list_n.Min();
        }


        public string Sentir(Case[,] foret){
            return foret[pos_l, pos_c].Odeur;
        }


        public string Regarder_luminosite(Case[,] foret){
            return foret[pos_l, pos_c].Luminosite;
        }

        public string Ressentir_vent(Case[,] foret){
            return foret[pos_l, pos_c].Vitesse_vent;
        }


        public void Jeter_pierre(char d, Foret foret){
            score -= 10;
            Console.WriteLine(name + " lance une pierre vers le " + d);
            if(d == 'N'){
                foret.Utilisation_de_roches(pos_l - 1, pos_c);
            }
            if(d == 'S'){
                foret.Utilisation_de_roches(pos_l + 1, pos_c);
            }
            if(d == 'W'){
                foret.Utilisation_de_roches(pos_l, pos_c - 1);
            }
            if(d == 'E'){
                foret.Utilisation_de_roches(pos_l, pos_c + 1);
            }
        }

        public bool Prendre_portail(Case[,] foret){
            return (foret[pos_l, pos_c].Type == "portail");
        }

        public int Pos_l{
            get{return pos_l;}
        }
        public int Pos_c{
            get{return pos_c;}
        }

        public string Name{
            get{return name;}
        }

        public int Score{
            get{return score;}
            set{score = value;}
        }


        //afficher les connaissances du joueur sur la carte ainsi que ses analyses
        public void Afficher_Memoire(){
            Console.WriteLine();
            for(int l = 0; l < dim_foret; l++){
                for(int c = 0; c < dim_foret; c++){
                    if(memoire[l,c,0] >= 0){
                        Console.Write("{0,3}", Convert.ToInt32(memoire[l,c,0]).ToString("D3"));
                    }
                    else{
                        Console.Write("nan");
                    }
                    if(memoire[l,c,3] == 0){
                        Console.Write(" 0 ");
                    }
                    else{
                        Console.Write("{0,2} ", Convert.ToInt32(memoire[l,c,3]).ToString("D2"));
                    }
                }
                Console.WriteLine();
                for(int c = 0; c < dim_foret; c++){
                    if(memoire[l,c,1] >= 0){
                        Console.Write("{0,3}", Convert.ToInt32(memoire[l,c,1]).ToString("D3"));
                    }
                    else{
                        Console.Write("nan");
                    }
                    if(memoire[l,c,4] == 0){
                        Console.Write(" 0 ");
                    }
                    else if(memoire[l,c,4] == -1){
                        Console.Write("   ");
                    }
                    else{
                        Console.Write(" 1 ");
                    }
                }
                Console.WriteLine();
                for(int c = 0; c < dim_foret; c++){
                    if(memoire[l,c,2] >= 0){
                        Console.Write("{0,3}", Convert.ToInt32(memoire[l,c,2]).ToString("D3"));
                    }
                    else{
                        Console.Write("nan");
                    }
                    if(memoire[l,c,5] == 0){
                        Console.Write("0");
                    }
                    else if(memoire[l,c,5] == -1){
                        Console.Write(" ");
                    }
                    else{
                        Console.Write("1");
                    }
                    if(memoire[l,c,6] == 0){
                        Console.Write("0 ");
                    }
                    else if(memoire[l,c,6] == -1){
                        Console.Write("  ");
                    }
                    else{
                        Console.Write("1 ");
                    }
                }
                Console.WriteLine("\n");
            }
            
        }

    }
}