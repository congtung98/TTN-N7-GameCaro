using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCaro
{
    public class Player
    {
        private string name;    //Ctrl + R + E

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }
        private Image mark;

        public Image Mark
        {
            get
            {
                return mark;
            }

            set
            {
                mark = value;
            }
        }

        public int Score
        {
            get
            {
                return score;
            }

            set
            {
                score = value;
            }
        }

        private int score;

        public Player(string name, Image mark, int score)
        {
            this.Name = name;
            this.Mark = mark;
            this.Score = score;
        }
    }
}
