using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Game : MonoBehaviour
{
    private static int SCREEN_WIDTH = 64;    //1024 pixels
    private static int SCREEN_HEIGHT = 48;   //768 pixels

    public float speed = 0.15f;
    private float timer=0;

    public bool simulationEnabled = false;

    public long simNR=0;  //number of generations, this should be used to add nota 6 functionality
    public long ct=0;     //generations number used in counter

    private bool button1Down = false, button2Down = false, button2Up = false;

    private int zoneX1 = 0, zoneY1 = 0;
    private int zoneX2 = 0, zoneY2 = 0;

    Cell[,] grid = new Cell[SCREEN_WIDTH, SCREEN_HEIGHT];

    private Vector2 _startPos;
    private Vector2 _endPos;
    private float _depth = -5f;
    private Material _material;
    private List<Rect> _rectangles = new List<Rect>();

    public Button buttonClear, buttonZone, buttonRandom, buttonCounter;
    public TMP_InputField generationsField;
    public TMP_Text counterField; 
    public Toggle simulationToggle;
    public Slider speedSlider;

    void Start(){
        PlaceCells();

        Shader shader = Shader.Find("Hidden/Internal-Colored");
        _material = new Material(shader);
        _material.hideFlags = HideFlags.HideAndDontSave;
        _material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        _material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        _material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

        
        simulationToggle.onValueChanged.AddListener(delegate {
                ToggleValueChanged(simulationToggle);
            });

        buttonClear.onClick.AddListener(ClearScreen);
        buttonRandom.onClick.AddListener(Randomize);
        buttonZone.onClick.AddListener(ClearZones);
        buttonCounter.interactable = false;
        generationsField.onEndEdit.AddListener(SetGenerations);
        speedSlider.maxValue = 0.4f;
        speedSlider.value = 0.15f;
        speedSlider.onValueChanged.AddListener(delegate {ChangeSpeed();});
    }

    void ToggleValueChanged(Toggle change)
    {
        simulationEnabled = change.isOn;
    }

    void ChangeSpeed(){
        speed = speedSlider.value;
    }

    void SetGenerations(string text){
        if(text == ""){
            simNR = 0;
            generationsField.text = "0";
        }
        else{
            simNR = long.Parse(text);
        long limit = 9999999999;
        if(simNR > limit){
            simNR = limit;
            generationsField.text = "9999999999";
        }
        else if(simNR <= 0){
            simNR = 0;
            generationsField.text = "0";
        }
        }
        
    }

    void OnRenderObject()
    {
        // Draw all the rectangles in the list
        _material.SetPass(0);
        GL.PushMatrix();
        GL.LoadOrtho();
        foreach (Rect rect in _rectangles)
        {
            GL.Begin(GL.QUADS);
            GL.Color(new Color(1f, 1f, 1f, 0.1f));
            GL.Vertex3(rect.xMin, rect.yMin, -5);
            GL.Vertex3(rect.xMax, rect.yMin, -5);
            GL.Vertex3(rect.xMax, rect.yMax, -5);
            GL.Vertex3(rect.xMin, rect.yMax, -5);
            GL.End();
        }
        GL.PopMatrix();
    }

    void Update(){
        userInput();

        if (Input.GetMouseButtonDown(1))
        {
            _startPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            _endPos = Input.mousePosition;
            // Convert the start and end positions of the rectangle to clip space
            Vector3 startClipPos = Camera.main.ScreenToViewportPoint(new Vector3(_startPos.x, _startPos.y, _depth));
            Vector3 endClipPos = Camera.main.ScreenToViewportPoint(new Vector3(_endPos.x, _endPos.y, _depth));
            Rect rect = new Rect(startClipPos.x, startClipPos.y, endClipPos.x - startClipPos.x, endClipPos.y - startClipPos.y);
            

            Vector2 strt = Camera.main.ScreenToWorldPoint(_startPos);
            Vector2 end = Camera.main.ScreenToWorldPoint(_endPos);
            int rectX1 = Mathf.RoundToInt(strt.x);
            int rectY1 = Mathf.RoundToInt(strt.y);
            int rectX2 = Mathf.RoundToInt(end.x);
            int rectY2 = Mathf.RoundToInt(end.y);

            if ((rectX1 <= SCREEN_WIDTH) && (rectX1 >= 0) 
                && (rectX2 <= SCREEN_WIDTH) && (rectX2 >= 0) 
                && (rectY1 <= SCREEN_HEIGHT) && (rectY1 >= 0) 
                && (rectY2 <= SCREEN_HEIGHT) && (rectY2 >= 0)) {_rectangles.Add(rect); }
        }

        if (((Input.GetAxis("Mouse X") != 0) || (Input.GetAxis("Mouse Y") != 0) )&& button1Down){
            Vector2 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            int x = Mathf.RoundToInt(mousePoint.x);
            int y = Mathf.RoundToInt(mousePoint.y);
             
            if (x>= 0 && y>= 0 && x<SCREEN_WIDTH && y<SCREEN_HEIGHT){
                //in bounds
                grid[x,y].SetAlive(true);
            }
        }

        if (button2Down && button2Up){
            int i,j,n,m,k,l;
            if (zoneX1 < zoneX2) {k = zoneX1; n = zoneX2;}
            else {k = zoneX2; n = zoneX1;}

            if (zoneY1 < zoneY2) {l = zoneY1; m = zoneY2;}
            else {l = zoneY2; m = zoneY1;}

            for (i = k; i <= n; i++){
                for (j = l; j <= m; j++)
                    grid[i,j].isInZone = true;
            }

            button2Down = false;
            button2Up = false;
        }

        if (simulationEnabled){
               if (timer>=speed){
                    timer = 0f;
                    CountNeighbors();
                    PopulationControl();
                    ct++;
                    counterField.text = ct.ToString();
                    if(ct>=simNR){
                        simulationEnabled=false;
                        simulationToggle.isOn = false;
                        ct=0;
                        return;
                    }
                }
                else{
                    timer += Time.deltaTime;
                }

                
        }
            
    }

    void OnDisable()
    {
        if (_material != null)
        {
            DestroyImmediate(_material);
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

    void ClearScreen(){
        int i,j;
            //remove all cells
        for (i = 0; i < SCREEN_WIDTH; i++){
            for (j = 0; j < SCREEN_HEIGHT; j++)
                grid[i,j].SetAlive(false);
        }
    }

    bool RandomAliveCell(){
        int rand = UnityEngine.Random.Range(0,100);
        
        if (rand> 75){
            return true;
        }
        return false;
    }

    void Randomize(){
        int i,j;
            for (i = 0; i < SCREEN_WIDTH; i++)
                for (j = 0; j < SCREEN_HEIGHT; j++)
                    grid[i,j].SetAlive(RandomAliveCell());
    }

    void ClearZones(){
        _rectangles.Clear();
        int i,j;
        //remove all zones
        for (i = 0; i < SCREEN_WIDTH; i++){
            for (j = 0; j < SCREEN_HEIGHT; j++)
                {grid[i,j].isInZone = false;}
        }
    }

    void userInput(){
        if (Input.GetMouseButtonDown(0)){
            button1Down = true;
            Vector2 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            int x = Mathf.RoundToInt(mousePoint.x);
            int y = Mathf.RoundToInt(mousePoint.y);
             
            if (x>= 0 && y>= 0 && x<SCREEN_WIDTH && y<SCREEN_HEIGHT){
                //in bounds
                grid[x,y].SetAlive(!grid[x,y].isAlive);
            }
        }

        if (Input.GetMouseButtonUp(0)){
            button1Down = false;
        }

        if (Input.GetMouseButtonDown(1)){
            Vector2 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            zoneX1 = Mathf.RoundToInt(mousePoint.x);
            zoneY1 = Mathf.RoundToInt(mousePoint.y);
            button2Down = true;
        }
        
        if (Input.GetMouseButtonUp(1)){
            Vector2 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            zoneX2 = Mathf.RoundToInt(mousePoint.x);
            zoneY2 = Mathf.RoundToInt(mousePoint.y);
            button2Up = true;

            if (zoneX1 > SCREEN_WIDTH) {button2Down = false; button2Up = false;}
            if (zoneX1 < 0) {button2Down = false; button2Up = false;}
            if (zoneX2 > SCREEN_WIDTH) {button2Down = false; button2Up = false;}
            if (zoneX2< 0) {button2Down = false; button2Up = false;}
            if (zoneY1 > SCREEN_HEIGHT) {button2Down = false; button2Up = false;}
            if (zoneY1 < 0) {button2Down = false; button2Up = false;}
            if (zoneY2 > SCREEN_HEIGHT) {button2Down = false; button2Up = false;}
            if (zoneY2 < 0) {button2Down = false; button2Up = false;}
        }


        if(Input.GetKeyUp(KeyCode.P)){
            //start/pause simulation
            simulationEnabled=(!simulationEnabled);
            simulationToggle.isOn = simulationEnabled;
        }
        if(Input.GetKeyUp(KeyCode.R)){
            //randomize cells
            Randomize();
        }
        if(Input.GetKeyUp(KeyCode.C)){
            ClearScreen();
        }
        if(Input.GetKeyUp(KeyCode.Z)){
            ClearZones();
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
                //Any cell alive with 1, 2 or 3 neighbors survives
                //Any dead cell with 3 or 7 alive neighbors rises from the dead
                //All other live cells die in the next generation and all other cells stay dead

                //long life rule zone
                if (grid[x,y].isInZone){
                    if(grid[x,y].isAlive){
                        if(grid[x,y].numNeighbors != 5){
                            grid[x,y].SetAlive(false);
                        }
                    }
                    else{
                        //dead cell
                        if (grid[x,y].numNeighbors==3 || grid[x,y].numNeighbors==4 || grid[x,y].numNeighbors==5){
                            grid[x,y].SetAlive(true);
                        }
                    }
                }
                else{
                    if(grid[x,y].isAlive){
                        if(grid[x,y].numNeighbors > 3){
                            grid[x,y].SetAlive(false);
                        }
                    }
                    else{
                        //dead cell
                        if (grid[x,y].numNeighbors==3 || grid[x,y].numNeighbors==7){
                            grid[x,y].SetAlive(true);
                        }
                    }

                }

                
            }
        }
    }



}