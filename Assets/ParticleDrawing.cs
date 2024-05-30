using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class ParticleDrawing : MonoBehaviour
{
    public int mapHeight;
    public int mapWidth;

    public int magnitude;

    public bool useRandomColors;
    public bool useRandomColor;
    public Color[] randomColors;

    public bool useMySeed;
    public float noiseSeed;
    public bool addVeloc;
    [Header("advanced noise")] public bool useAdvancedMethod;
    public int advancedSeed;
    public int octavs;
    public float lacunarity;
    public float persistance;
    public float scale;
    public Vector2 offset;

    [Header("change value over time?")] public bool changeFieldOverTime;
  
    public float changeValue;
    public float changeUpdateTime;
    public bool delayToStartChangeValue;
    public float changeValueStartDelay;

    


    [Header("to show the vector field")] public bool showVectorField;
    public GameObject line;
    public GameObject parent;

    [Header("to show the noise on a texture")]
    public Renderer textureRendered;

    private GameObject[,] lines;
    private float[,] noiseMap;

    private ParticleSystem.Particle[] _particles;

    private ParticleSystem pSystem;

    // Start is called before the first frame update
    void Start()
    {
        lines = new GameObject[mapWidth, mapHeight];
        noiseMap = new float[mapWidth, mapHeight];
        pSystem = GetComponent<ParticleSystem>();
        _particles = new ParticleSystem.Particle[pSystem.main.maxParticles];
        pSystem.GetParticles(_particles);
        if (useRandomColor)
            SetVeryRandomColor();
        //pSystem.simulationSpace = ParticleSystemSimulationSpace.World;
        if(useAdvancedMethod)
            noiseMap = GenerateNoiseMap(mapWidth, mapHeight, advancedSeed, scale, octavs, persistance, lacunarity, offset);
        else
        {
            noiseMap = GenerateNoiseMap(mapWidth, mapHeight);
        }

        if (showVectorField)
            ShowVectorField();

        InvokeRepeating("ChangeNoiseMapValues", changeUpdateTime, changeUpdateTime);
        if (delayToStartChangeValue)
        {
            Invoke("DelayForChangingValue", changeValueStartDelay);
            changeFieldOverTime = false;
        }
        var col = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
        DrawTexture(TextureFromHeightMap(noiseMap));
    }

    void DelayForChangingValue()
    {
        changeFieldOverTime = true;
    }

    void ShowVectorField()
    {
        foreach (var o in lines)
        {
            Destroy(o);
        }

        if(useAdvancedMethod)
            noiseMap = GenerateNoiseMap(mapWidth, mapHeight, advancedSeed, scale, octavs, persistance, lacunarity, offset);
        else
        {
            noiseMap = GenerateNoiseMap(mapWidth, mapHeight);
        }
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                var theLine = Instantiate(line, new Vector3(j, -i), Quaternion.identity, parent.transform);
                theLine.transform.Rotate(0, 0, noiseMap[i, j] * 360);
                lines[i, j] = theLine;
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            //ShowVectorField();
            noiseMap = GenerateNoiseMap(mapWidth, mapHeight, advancedSeed, scale, octavs, persistance, lacunarity, offset);
            DrawTexture(TextureFromHeightMap(noiseMap));

        }
    }

 
    void ChangeNoiseMapValues()
    {
        if (!changeFieldOverTime)
            return;

        
        for (int i = 0; i < noiseMap.GetLength(0); i++)
        {
            for (int j = 0; j < noiseMap.GetLength(1); j++)
            {
                
                    noiseMap[i, j] += changeValue;
                 

                //rly??
                if (noiseMap[i, j] >= 1)
                {
 
                       
                    noiseMap[i, j] = 0;
                }

                if (noiseMap[i, j] < 0)
                {
           
                    noiseMap[i, j] = 1;
                }


                if (showVectorField)
                    lines[i, j].transform.Rotate(0, 0, changeValue * 360);
            }
        }

    }

    void SetVeryRandomColor()
    {
        pSystem.startColor =
            new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
    }

    void SetRandomColor()
    {
        pSystem.startColor = randomColors[Random.Range(0, randomColors.Length)];
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (useRandomColors)
            SetRandomColor();
        //print(Mathf.PerlinNoise(crawlingVector.x, crawlingVector.y));
        var maxParticles = pSystem.GetParticles(_particles);

        for (int i = 0; i < maxParticles; i++)
        {
            if (_particles[i].position.x >= mapWidth)
                _particles[i].position = new Vector3(0, _particles[i].position.y);
            if (_particles[i].position.x < 0)
                _particles[i].position = new Vector3(mapWidth - 1, _particles[i].position.y);
            if (_particles[i].position.y >= mapHeight)
                _particles[i].position = new Vector3(_particles[i].position.x, 0);
            if (_particles[i].position.y < 0)
                _particles[i].position = new Vector3(_particles[i].position.x, mapHeight - 1);

            var x = (int) _particles[i].position.x;
            var y = (int) _particles[i].position.y;
            if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
            {
                var noise = noiseMap[x, y]; //myNoiseMap[x, y];//
                var vec = GetAngleFromVallue(noise);

                if (addVeloc)
                    _particles[i].velocity += vec;
                else
                {
                    _particles[i].velocity = vec;
                }

                _particles[i].velocity = _particles[i].velocity.normalized * magnitude;
            }
        }

        pSystem.SetParticles(_particles);
    }

    public float[,] GenerateNoiseMap(int mapWidth, int mapHeight)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];
        /*if (!useMySeed)
            noiseSeed = Random.Range(0, 10.0f);*/
        if (!useMySeed)
            noiseSeed = Random.Range(0, 1.0f); //big numbers  will change the noise drastically :D
        print("seed: " + noiseSeed + "how to use it??");
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                //noiseMap[x, y] = Mathf.PerlinNoise(x/(float)mapWidth*noiseSeed  ,y/(float)mapHeight*noiseSeed );
                noiseMap[x, y] = Mathf.PerlinNoise(x / (float) mapWidth + noiseSeed, y / (float) mapHeight + noiseSeed);
            }
        }

        return noiseMap;
    }

    public float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves,
        float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;


        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);// * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }

    Vector3 GetAngleFromVallue(float value)
    {
        var degVal = value * 360;

        degVal *= Mathf.Deg2Rad;
        var y = Mathf.Sin(degVal);
        var x = Mathf.Cos(degVal);
        return new Vector2(-y, x); //yeaaaah!
    }


    public void DrawTexture(Texture2D texture)
    {
        //using shared one instead of the mat, helps us to see this in the scene before starting the game
        textureRendered.sharedMaterial.mainTexture = texture;
        textureRendered.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public Texture2D TextureFromHeightMap(float[,] heightMap)
    {
        var width = heightMap.GetLength(0);
        var height = heightMap.GetLength(1);

        var texture = new Texture2D(width, height);

        var colorMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
            }
        }

        return TextureFromClorMap(colorMap, width, height);
    }

    public static Texture2D TextureFromClorMap(Color[] colorMap, int width, int height)
    {
        var texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }
}