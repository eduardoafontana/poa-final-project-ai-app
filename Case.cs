using System;

namespace TD3
{
    public class Case
    {
        private string odeur;
        private string vitesse_vent;
        private string luminosite;
        private string name;
        private string type;

        public Case(string name, string type)
        {
            this.name = name;
            this.type = type;           //portail || monstre || crevasse || vide
            odeur = "neutre";           //mauvaise || neutre
            vitesse_vent = "faible";    //faible || fort
            luminosite = "faible";      //faible || forte
        }

        public string Name{
            get{return this.name;}
        }

        public string Odeur{
            set { odeur = value; }
            get { return this.odeur; }
        }

        public string Vitesse_vent{
            set { vitesse_vent = value; }
            get { return this.vitesse_vent; }
        }

        public string Luminosite{
            set { luminosite = value; }
            get { return this.luminosite; }
        }
        public string Type{
            set { type = value; }
            get { return this.type; }
        }

        public override string ToString()
        {
            string r = "";
            if(type == "portail"){
                r = "O";
            }
            else if(type == "monstre"){
                r = "M";
            }
            else if(type == "crevasse"){
                r = "v";
            }
            else{
                r = ".";
            }
            return r;
        }

        
    }
}
