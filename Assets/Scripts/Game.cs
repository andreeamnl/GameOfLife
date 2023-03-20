using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    private static int SCREEN_WIDTH = 64;    //1024 pixels
    private static int SCREEN_HEIGHT = 48;   //768 pixels

    public float speed = 0.1f;
    private float timer=0;

    public bool simulationEnabled = false;

    public int simNR=2000;  //number of generations, this should be used to add nota 6 functionality


    Cell[,] grid = new Cell[SCREEN_WIDTH, SCREEN_HEIGHT];

    void Start(){
        PlaceCells();
    }
    void Update(){
        userInput();

        if (simulationEnabled){
               if (timer>=speed){
                    timer = 0f;
                    CountNeighbors();
                    PopulationControl();
                }else{
                    timer += Time.deltaTime;
                }

            }
        }



    void PlaceCells(){
        for(int y = 0; y<SCREEN_HEIGHT;y++){
            for(int x=0; x<SCREEN_WIDTH; x++){
                Cell cell=Instantiate(Resources.Load("Prefabs/Cell", typeof(Cell)), new Vector2(x,y), Quaternion.identity) as Cell;
                grid[x,y]=cell;
                grid[x,y].SetAlive(RandomAliveCell());
            }
        }
    }

    bool RandomAliveCell(){
        int rand = UnityEngine.Random.Range(0,100);
        
        if (rand> 55){
            return true;
        }
        return false;
    }

    void userInput(){
        if (Input.GetMouseButtonDown(0)){
            Vector2 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            int x = Mathf.RoundToInt(mousePoint.x);
            int y = Mathf.RoundToInt(mousePoint.y);
             
            if (x>= 0 && y>= 0 && x<SCREEN_WIDTH && y<SCREEN_HEIGHT){
                //in bounds
                grid[x,y].SetAlive(!grid[x,y].isAlive);
            }
        }
        if(Input.GetKeyUp(KeyCode.P)){
            //pause simulation
            simulationEnabled=false;
        }
        if(Input.GetKeyUp(KeyCode.B)){
            //build simulation/resume
            simulationEnabled=true;
        }
        if(Input.GetKeyUp(KeyCode.C)){
            //first try at changing the generations number through keycode user input
            simNR=10;
        }
    }


    void CountNeighbors(){
        for (int  y =0; y<SCREEN_HEIGHT;y++){
            for (int x = 0; x<SCREEN_WIDTH; x++){
                int numNeighbors = 0;

                //North
                if (y+1<SCREEN_HEIGHT){
                    if(grid[x,y+1].isAlive){
                        numNeighbors++;
                    }

                }

                //East
                if (x+1<SCREEN_HEIGHT){
                    if(grid[x+1,y].isAlive){
                        numNeighbors++;
                    }
                }

                //South
                if (y-1>=0){
                    if(grid[x,y-1].isAlive){
                        numNeighbors++;
                    }
                }

                //West
                if(x-1>=0){
                    if(grid[x-1,y].isAlive){
                        numNeighbors++;
                    }
                }

                //NortEast

                if(x+1<SCREEN_WIDTH && y+1<SCREEN_HEIGHT ){
                    if (grid[x+1,y+1].isAlive){
                        numNeighbors++;
                    }
                }

                //NorthWest

                if(x-1>=0 && y+1<SCREEN_HEIGHT){
                    if(grid[x-1,y+1].isAlive){
                        numNeighbors++;
                    }
                }

                //SouthEast

                if(x+1<SCREEN_WIDTH && y-1>=0){
                    if (grid[x+1,y-1].isAlive){
                        numNeighbors++;
                    }
                }


                //SouthWest

                if(x-1>=0 && y-1>=0){
                    if(grid[x-1,y-1].isAlive){
                        numNeighbors++;
                    }
                }

                grid[x,y].numNeighbors=numNeighbors;
            }
        }
    }
    
    void PopulationControl(){
        for (int y=0; y<SCREEN_HEIGHT; y++){
            for(int x=0; x<SCREEN_WIDTH; x++){
                //Any cell alive with 2 or 3 neighbors survives
                //Any dead cell with 3 alive neighbors rises from the dead
                //All other live cells die in the next generation and all other cells stay dead
                if(grid[x,y].isAlive){
                    if(grid[x,y].numNeighbors != 2 && grid[x,y].numNeighbors !=3){
                        grid[x,y].SetAlive(false);
                    }
                }
                else{
                    //dead cell
                    if (grid[x,y].numNeighbors==3){
                        grid[x,y].SetAlive(true);
                    }
                }
            }
        }
    }



}