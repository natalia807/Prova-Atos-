using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Atividade
{
    public partial class frmAtividade : Form
    {
        public frmAtividade()
        {
            InitializeComponent();
        }



        private void btnRun_Click(object sender, EventArgs e)
        {
            if (txtArquivo.Text.Trim().Equals(""))
            {
                MessageBox.Show(this, "Caminho do arquivo deve ser informado");
                txtArquivo.Focus();
                return;
            }

            if (!File.Exists(txtArquivo.Text.Trim()))
            {
                MessageBox.Show(this, "Arquivo inexistente!");
                txtArquivo.Focus();
                return;
            }

            Thread thread = new Thread(() => ExecutaAtividade(txtArquivo.Text.Trim()));
            thread.Name = "Atividade - Run";
            thread.Start();
        }


        private void ExecutaAtividade(string filePath)
        {
            this.Invoke(new MethodInvoker(delegate()
            {
                txtArquivo.Enabled = false;
                btnRun.Enabled = false;
            }));

            try
            {
                CodigoAtividade(filePath);

                this.Invoke(new MethodInvoker(delegate()
                {
                    MessageBox.Show(this, "Finalizado!");
                }));
            }
            catch (Exception ex)
            {
                this.Invoke(new MethodInvoker(delegate()
                {
                    MessageBox.Show(this, ex.Message);
                }));
            }
            finally
            {
                this.Invoke(new MethodInvoker(delegate()
                {
                    txtArquivo.Enabled = true;
                    btnRun.Enabled = true;
                }));
            }
        }
        
        private class Possibility{
            public int direction;
            public Boolean visited;
            
            /*class constructor
             * @params
             * int
             * Boolean
            */
            public Possibility(int direction, Boolean visited){
                this.setDirection(direction);
                this.setVisited(visited);
            }
            
            public int getDirection() => this.direction;
            public Boolean getVisited() => this.visited;
            public void setDirection(int direction) => this.direction = direction;
            public void setVisited(Boolean visited) => this.visited = visited;         
            
        }

        private class Crossroad{
            
            public int positionX { get ;}
            public int positionY { get ;}
            private List<Possibility> possibilities;

            /*class constructor
             * @params
             * int []
            */
            public Crossroad(int [] position){
                possibilities = new List<Possibility>();
                this.positionX = position[0];
                this.positionY = position[1];

            }

            //function to add a possibility
            public void AddPossibilities(int p, Boolean v){
                Possibility possibility = new Possibility(p, v);
                possibilities.Add(possibility);
            }

            //function to remove a possibility
            public Boolean removePossibilities(Possibility p){
                if(possibilities.Remove(possibilities.Where(a => a.getDirection() == p.getDirection()).FirstOrDefault()))
                    return true;
                else return false;
            }


            
            public List<Possibility> getPossibilities() => this.possibilities;

            private void setPossiblities(List<Possibility> possibilities) => this.possibilities = possibilities;



        }

        private class Labrinth{

            private string filePath;
            private int [] startPoint;
            private int [] length;
            private int facing = -1;
            private string [,] maze;
            private ArrayList navigation;
            private List<Crossroad> crossroads;
            private string fileName {get ; set ;}

            /*class constructor
             * @params
             * string
            */
            public Labrinth(string filePath){
                this.navigation = new ArrayList();
                this.crossroads = new List<Crossroad>();
                LerArquivo(filePath);                
            }

            // void function to read the file with a file stream and separate the lines and treat them
            private void LerArquivo(string filePath){
                int count = 0;
                int count2 = 0;
                string [,] temp;

                //getting name and path
                this.fileName = Path.GetFileNameWithoutExtension(filePath);
                this.filePath = filePath;

                // using file stream to read the file
                using (var stream = File.OpenRead(filePath)){
                    using (var reader = new StreamReader(stream)){  
                        this.setLength(reader.ReadLine());
                        temp = new string [this.length[0], this.length[1]];
                        string line;
                        while (!(String.IsNullOrEmpty(line = reader.ReadLine()))){
                            count2=0;
                            string[] aux = line.Split(' ');
                            foreach(string a in aux){
                                temp[count, count2] = a;
                                count2++;
                            }
                            count++;
                        }
                        
                        this.setMaze(temp);
                    }
                }
            }

            public void SalvarArquivo(){
                //breaking apart the path input from the user so we can use it as is to save the file
                string pathWithoutName = Path.GetDirectoryName(this.getFilepath());
                MessageBox.Show("Salvando o arquivo no diretório  :  "+ pathWithoutName);
                System.IO.File.WriteAllLines((pathWithoutName  + "\\saida-" + this.fileName + ".txt" ), this.getNavigation().Cast<string>());
                MessageBox.Show("Arquivo salvo com sucesso com nome : \n" +  "saida-" + this.fileName + ".txt");
            }

            //function to locate the starting point of each maze
            private int[] StartPoint(){
                int [] aux = {-1,-1};
                for(int i = 0; i < this.length[0]; i++)
                    for(int j = 0; j < this.length[1]; j++)
                        if(this.maze[i,j] == "X"){
                            aux[0] = i;
                            aux[1] = j;
                            return aux;
                        }
                return aux;
            }

            private void LookArround(int[] position){
                
                //function to look arround and decide were to go from where you came
                Crossroad cross = new Crossroad(position);

                for (int i=0; i<4; i++)
                
                    switch(i){
                        // each of these cases have the same proprieties and the only difference is the direction they are facing (0 north , 1 West, 2 East , 3 South)
                        case 0:
                            //cheking if the current position is within limits of the array
                            if(position[0] - 1 >= 0){
                                //checking if the position north of the actual position is a path (0) or is an X wich marks the initial position
                                if(this.maze[position[0]-1 , position[1]].Equals("0") || this.maze[position[0]-1 , position[1]].Equals("X")){
                                    //crossroad where the position x and y equals the northmost position if it exists so we can block useless paths
                                    Crossroad c = this.crossroads.Where(p => p.positionX == position[0]-1 && p.positionY == position[1]).FirstOrDefault();
                                    //cheking if the list isn't empty, and cheking the last crosroad passed, if it is equal to the position at north, it means that it is a path that has been walked before
                                    if( this.crossroads.Count>0 && ( this.crossroads.Last().positionX == (position[0]-1) && this.crossroads.Last().positionY == position[1] ) ){
                                        cross.AddPossibilities(i, true);
                                        break;
                                    // else if c isn't defined null or not initialized
                                    } else if(c != null){
                                        // create a possibility that is going to be removed from the position at the north of this one so it can't make you walk the same paths over and over again
                                        Possibility poss = new Possibility(3, false);
                                        if(c.removePossibilities(poss))
                                            break;
                                        else 
                                            break;
                                    }
                                    //this else means that is an path that hasn't been walked before and is a valid way
                                    else {
                                        cross.AddPossibilities(i, false);
                                        break;
                                    }
                                } else break;
                            } else break;
                        
                        case 1: 
                            if(position[1] - 1 >= 0){

                                Crossroad c = this.crossroads.Where(p => p.positionX == position[0] && p.positionY == position[1]-1).FirstOrDefault();

                                if(this.maze[position[0], position[1]-1].Equals("0") || this.maze[position[0], position[1]-1].Equals("X")){
                                    if(this.crossroads.Count>0 && ( this.crossroads.Last().positionX == position[0] && this.crossroads.Last().positionY == (position[1]-1) ) ){
                                        cross.AddPossibilities(i, true);
                                        break;
                                    } else if(c != null){
                                        Possibility poss = new Possibility(2, false);
                                        if(c.removePossibilities(poss))
                                            break;
                                        else 
                                            break;
                                    }
                                    else {
                                        cross.AddPossibilities(i, false);
                                        break;
                                    }
                                } else break;
                            } else break;

                        case 2: 
                            if(position[1] + 1 < this.length[1]){
                                if(this.maze[position[0], position[1]+1].Equals("0") || this.maze[position[0], position[1]+1].Equals("X")){

                                    Crossroad c = this.crossroads.Where(p => p.positionX == position[0] && p.positionY == position[1] + 1).FirstOrDefault();

                                    if(this.crossroads.Count>0 && ( this.crossroads.Last().positionX == (position[0]) && this.crossroads.Last().positionY == (position[1] + 1) ) ){
                                        cross.AddPossibilities(i, true);
                                        break;
                                    } else if(c != null){
                                        Possibility poss = new Possibility(1, false);
                                        if(c.removePossibilities(poss))
                                            break;
                                        else 
                                            break;
                                    }
                                    else {
                                        cross.AddPossibilities(i, false);
                                        break;
                                    }
                                } else break;
                            } else break;

                        case 3: 
                            if(position[0] + 1 < this.length[0]){
                                if(this.maze[position[0]+1, position[1]].Equals("0") || this.maze[position[0]+1, position[1]].Equals("X")){

                                    Crossroad c = this.crossroads.Where(p => p.positionX == position[0]+1 && p.positionY == position[1]).FirstOrDefault();

                                    if(this.crossroads.Count>0 && ( this.crossroads.Last().positionX == (position[0]+1) && this.crossroads.Last().positionY == position[1] ) ){
                                        cross.AddPossibilities(i, true);
                                        break;
                                    } else if(c != null){
                                        Possibility poss = new Possibility(0, false);
                                        if(c.removePossibilities(poss))
                                            break;
                                        else 
                                            break;
                                    }
                                    else {
                                        cross.AddPossibilities(i, false);
                                        break;
                                    }
                                } else break;
                            } else break;

                        default:
                            break;
                        
                    }
                    //adding the crossroad in the list
                    this.crossroads.Add(cross);
            }

            // function that moves acording to a set priority defined at funtion LookArround
            private Boolean MoveFoward(int[] local){
                int [] aux = {this.crossroads[this.crossroads.Count-1].positionX , this.crossroads[this.crossroads.Count-1].positionY};
                //validation to make sure we are at the same postition as it is stored in crossroads[].getPosition()
                if (aux[0] == local[0] && aux[1] == local[1]){
                    // validation to make sure we have at least one possibility to move when in a crossroad
                    if(this.crossroads[this.crossroads.Count - 1].getPossibilities().Count > 0){
                        //setting facing var with the foremost possibility
                        this.setFacing(this.crossroads[this.crossroads.Count - 1].getPossibilities()[0].getDirection());
                        // using facing to determinate direction of movement
                        switch(this.getFacing()){
                        //in case x walks towards that direction and remove the possibility from the array.
                            case 0:
                                local[0]--;
                                this.crossroads[this.crossroads.Count - 1].getPossibilities().RemoveAt(0);
                                return true;

                            case 1:
                                local[1]--;
                                this.crossroads[this.crossroads.Count - 1].getPossibilities().RemoveAt(0);
                                return true;

                            case 2:
                                local[1]++;
                                this.crossroads[this.crossroads.Count - 1].getPossibilities().RemoveAt(0);
                                return true;

                            case 3:
                                local[0]++;
                                this.crossroads[this.crossroads.Count - 1].getPossibilities().RemoveAt(0);
                                return true;

                            default:
                                return false;
                        }
                    } else return false;

                } else return false;
            }

            private Boolean Choice(int[] current){
                //checking if we finished the maze
                if(current[0] - 1 < 0 || current[1] - 1 < 0 || current[0] + 1 >= this.length[0] || current[1] + 1 >= this.length[1])
                    return false;

                //cheking if have more than 1 direction to go
                if (this.crossroads[this.crossroads.Count()-1].getPossibilities().Count().Equals(1)){ 
                    if(this.MoveFoward(current)){
                        this.crossroads.RemoveAt(this.crossroads.Count() - 1);
                        return true;
                    } else return false;
                } else {            //this implies that we have no routes or have more than 1 route in this crossroad
                    // checks if still has possibilities , and if dont go to the last possible position in the stack and starts backtracking
                    if(this.crossroads[this.crossroads.Count() - 1].getPossibilities().Count() == 0){
                        this.crossroads.RemoveAt(this.crossroads.Count() - 1);
                        current[0] = this.crossroads[this.crossroads.Count() - 1].positionX;
                        current[1] = this.crossroads[this.crossroads.Count() - 1].positionY;
                        return true;
                    }
                    //this checks if the most prioritized option is already visited and if not move to it
                    if(!this.crossroads[this.crossroads.Count() - 1].getPossibilities()[0].getVisited())
                        return this.MoveFoward(current);
                    else{ 
                        //this take the most prioritized option that have been visited and put it in the end of list of possibilities
                        // in that way even if the others are all dead ends, this path will be available to backtrack;
                            Possibility aux = new Possibility(
                                this.crossroads[this.crossroads.Count() - 1].getPossibilities()[0].getDirection(), 
                                this.crossroads[this.crossroads.Count() - 1].getPossibilities()[0].getVisited());

                            this.crossroads[this.crossroads.Count() - 1].getPossibilities().RemoveAt(0);
                            this.crossroads[this.crossroads.Count() - 1].getPossibilities().Add(aux);
                            return this.MoveFoward(current);
                    }
                }
            }

            private Boolean Move(int[] current){
                //facing =  0 is North
                //facing =  1 is West
                //facing =  2 is East
                //facing =  3 is South
                
                // checking if this is the first interaction
                if (!this.facing.Equals(-1)){  
                    //temporary variable to compare values 
                    int [] temp = {this.crossroads[this.crossroads.Count-1].positionX , this.crossroads[this.crossroads.Count-1].positionY};
                    // checking if the actual position is the one already mapped
                    if((temp[0] == current[0]) && (temp[1] == current[1]))
                       return this.Choice(current);
                    else {      //this implies that this position possibilities haven't been mapped yet
                        LookArround(current);
                        return this.Choice(current);
                    }
                } else {
                    //first movement
                    LookArround(current);
                    this.setFacing(this.crossroads[this.crossroads.Count() - 1].getPossibilities()[0].getDirection());
                    return this.MoveFoward(current);
                }
                
            }
            

            //function to navigate in the matrix using left hand algorithm to leave mazes.
            public void Navigate(){

                ArrayList route = new ArrayList();
                Boolean hasRoute = true;
                this.setStartPoint(this.StartPoint());
                int [] current = this.getStartPoint();

                // Adding the starting point on the result list
                string transition = "O ["+ (current[0]+1) + ", " + (current[1]+1) + "]";
                route.Add(transition); 

                //while boolean hasRoute that only will be false if the we got out of the maze
                while(hasRoute){
                    hasRoute = this.Move(current);
                    if(hasRoute)
                        switch (this.getFacing()){
                            case 0:
                                transition = "C ["+ (current[0]+1) + ", " + (current[1]+1) + "]";
                                route.Add(transition);
                                break;
                            case 1:
                                transition = "E ["+ (current[0]+1) + ", " + (current[1]+1) + "]";
                                route.Add(transition);
                                break;
                            case 2:
                                transition = "D ["+ (current[0]+1) + ", " + (current[1]+1) + "]";
                                route.Add(transition);
                                break;
                            case 3:
                                transition = "B ["+ (current[0]+1) + ", " + (current[1]+1) + "]";
                                route.Add(transition);
                                break;
                            default:
                                break;
                        }   
                }

                this.setNavigation(route);

            }

            //getters and setters
            public string getFilepath() => this.filePath;

            public int [] getLength() => this.length;
            
            public int [] getStartPoint() => this.startPoint;

            public string [,] getMaze() => this.maze;

            public ArrayList getNavigation() => this.navigation;
            
            public int getFacing() => this.facing;


            private void setFilePath(string filePath) => this.filePath = filePath;

            private void setLength(string length){
                //more elaborate setter that already do the parssing needed to separate the length
                string[] aux = length.Split(' ');
                this.length = aux.Select(int.Parse).ToArray();
            }

            
            private void setStartPoint(int [] startPoint) => this.startPoint = startPoint;

            private void setMaze(string [,] maze) => this.maze = maze;

            private void setNavigation(ArrayList navigation) => this.navigation = navigation;
            
            private void setFacing(int facing) => this.facing = facing;
        
        }


        private void CodigoAtividade(string filePath){

            Labrinth labrinth = new Labrinth(filePath);
            labrinth.Navigate();
            MessageBox.Show("As dimensões do labirinto são : " + labrinth.getLength()[0] + " x " + labrinth.getLength()[1] +
                        "\nO ponto de partida é a posição : [" +labrinth.getStartPoint()[0] + " , " + labrinth.getStartPoint()[1] + "]" +
                        "\nA saída se encontra na posição de movimento : " + labrinth.getNavigation()[labrinth.getNavigation().Count -1] );
            labrinth.SalvarArquivo();
        }


    }
}
