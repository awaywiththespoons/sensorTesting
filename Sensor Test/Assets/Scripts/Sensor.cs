using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class Sensor : MonoBehaviour
{
    [Serializable]
    public class Knowledge
    {
        public List<Token> tokens = new List<Token>();
    }

    [Serializable]
    public class Token
    {
        public int id;
        public Vector3 feature; // mean triangle from training
        public float directionOffset;

        public List<Vector3> training = new List<Vector3>();
    }

    public struct TouchPattern
    {
        public int count;
        public Vector2 a, b, c;
    }

    public struct Frame
    {
        public Token token;
        public Vector2 position;
        public float direction;

        public TouchPattern pattern;
    }

    public struct Data
    {
        public Token token;
        public Vector3 feature;
    }

    public event Action OnTokenPlaced = delegate { };
    public event Action OnTokenLifted = delegate { };
    public event Action<Token> OnTokenClassified = delegate { };
    public event Action<Frame> OnTokenTracked = delegate { };

    public static float PolarAngle(Vector2 point)
    {
        return (Mathf.Atan2(point.y, point.x) * Mathf.Rad2Deg + 360) % 360;
    }

    public Token training { get; private set; }
    public Token detected { get; private set; }

    private float tokenTimeout;
    private List<Token> classifications = new List<Token>();

    public Knowledge knowledge = new Knowledge();
    public Queue<Frame> history = new Queue<Frame>();
    public List<Data> allTraining = new List<Data>();

    [SerializeField]
    private GameObject debug;

    public void SetTraining(Token token)
    {
        knowledge.tokens.Add(token);
        training = token;
    }

    public void SetClassify()
    {
        training = null;
        detected = null;
        classifications.Clear();
    }

    public void SaveTraining()
    {
        System.IO.File.WriteAllText(Application.persistentDataPath + "/training.json", JsonUtility.ToJson(knowledge));
    }

    public void LoadTraining()
    {
        Reset();

        string data = System.IO.File.ReadAllText(Application.persistentDataPath + "/training.json");

        knowledge = JsonUtility.FromJson<Knowledge>(data);

        ReduceNoise();

        foreach (var token in knowledge.tokens)
        {
            foreach (var point in token.training)
            {
                allTraining.Add(new Data { token = token, feature = point });
            }
        }
    }

    public void Reset()
    {
        SetClassify();

        knowledge.tokens.Clear();
        history.Clear();
        allTraining.Clear();
    }

    private void Start()
    {
        LoadTraining();
    }

    private void Update()
    {
        // get touches, scale to centimetres and sort clockwise
        TouchPattern pattern;
        pattern.count = Input.touchCount;
        pattern.a = pattern.count >= 1 ? Input.GetTouch(0).position / Screen.dpi * 2.54f : default(Vector2);
        pattern.b = pattern.count >= 2 ? Input.GetTouch(1).position / Screen.dpi * 2.54f : default(Vector2);
        pattern.c = pattern.count >= 3 ? Input.GetTouch(2).position / Screen.dpi * 2.54f : default(Vector2);

        IntegrateTouchPattern(pattern);
        
        if (detected != null && pattern.count < 2)
        {
            tokenTimeout += Time.deltaTime;
        }
        else
        {
            tokenTimeout = 0;
        }

        if (tokenTimeout > .25f)
        {
            tokenTimeout = 0;
            detected = null;
            classifications.Clear();
            // TODO: reset classification too

            OnTokenLifted();
        }

        if (pattern.count >= 5)
        {
            debug.SetActive(true);
        }
    }

    public void IntegrateTouchData(IList<Vector2> touches)
    {
        TouchPattern pattern;
        pattern.count = touches.Count;
        pattern.a = pattern.count >= 1 ? touches[0] / Screen.dpi * 2.54f : default(Vector2);
        pattern.b = pattern.count >= 2 ? touches[1] / Screen.dpi * 2.54f : default(Vector2);
        pattern.c = pattern.count >= 3 ? touches[2] / Screen.dpi * 2.54f : default(Vector2);

        IntegrateTouchPattern(pattern);
    }

    public void IntegrateTouchPattern(TouchPattern pattern)
    {
        SortPatternClockwise(ref pattern);

        if (training != null)
        {
            // if we're training and have exactly three touches, add this
            // pattern as an example of the token we're training
            if (pattern.count == 3)
            {
                TrainToken(training, pattern);
            }
        }
        else if (detected == null && pattern.count == 3)
        {
            Token classification = ClassifyPattern(pattern);

            if (classification != null)
            {
                classifications.Add(classification);
            }

            if (classifications.Count >= 16)
            {
                var best = knowledge.tokens.OrderByDescending(token => classifications.Count(data => data == token))
                                           .First();

                detected = best;
                classifications.Clear();

                OnTokenClassified(detected);
            }
        }

        if (training == null && detected != null)
        {
            // if we have already detected a token, we can try to reconstruct
            // the missing touch from the known points and expected shape
            if (pattern.count == 2)
            {
                // TODO: reconstruct the missing touch - use history to 
                // determine which possible reconstruction is most likely
                pattern.c = (pattern.b + pattern.a) * 0.5f;
                pattern.count = 3;
            }

            // limit history size
            // TODO: actually use history to assist orientation
            while (history.Count >= 16)
            {
                history.Dequeue();
            }
            
            history.Enqueue(OrientToken(detected, pattern));

            OnTokenTracked(history.Last());
        }
    }

    /// <summary>
    /// Add an example of touch pattern that is seen when the given token is
    /// on screen
    /// </summary>
    public void TrainToken(Token token, TouchPattern pattern)
    {
        Vector3 feature = ExtractSidesFeature(pattern);

        // if this isn't the first data-point, cycle the triangle side order
        // to best match the existing feature
        if (token.training.Count > 0)
        {
            feature = Triangle.CycleToMatch(feature, token.feature);
        }

        token.training.Add(feature);

        allTraining.Add(new Data { token = token, feature = feature });

        // update token's average observed feature (side lengths)
        int count = token.training.Count;
        token.feature = (token.feature * (count - 1) + feature) / count;
    }

    /// <summary>
    /// sort the position in the touch pattern so the corners of the triangle
    /// occur in a clockwise order
    /// </summary>
    public static void SortPatternClockwise(ref TouchPattern pattern)
    {
        Vector2 centroid = (pattern.a + pattern.b + pattern.c) / 3f;

        var touches = new List<Vector2> { pattern.a, pattern.b, pattern.c };
        var sorted = touches.OrderBy(touch => -PolarAngle(touch - centroid)).ToList();

        pattern.a = sorted[0];
        pattern.b = sorted[1];
        pattern.c = sorted[2];
    }

    /// <summary>
    /// Convert a pattern of three touches into a Vector3 of side lengths of a
    /// triangle
    /// </summary>
    public Vector3 ExtractSidesFeature(TouchPattern pattern)
    {
        var triangle = new Vector3((pattern.b - pattern.a).magnitude,
                                   (pattern.c - pattern.b).magnitude,
                                   (pattern.a - pattern.c).magnitude);

        // line-like triangles don't have a reliable concept of clockwise
        // winding, so instead we sort the sides by length to normalise the
        // feature
        if (Triangle.IsLine(triangle))
        {
            triangle = Triangle.SortSides(triangle);
        }

        return triangle;
    }

    private Vector3 Quantize(Vector3 feature, float factor = 10f)
    {
        feature.x = Mathf.Round(feature.x * factor) / factor;
        feature.y = Mathf.Round(feature.y * factor) / factor;
        feature.z = Mathf.Round(feature.z * factor) / factor;

        return feature;
    }

    /// <summary>
    /// TODO: annihilate training points that are too close to each other but
    /// representing different tokens
    /// 
    /// also kill training points that don't have any friends nearby?
    /// 
    /// maybe we can assume a single cluster - kill anything too far from the
    /// mean?
    /// </summary>
    public void ReduceNoise()
    {
        foreach (Token token in knowledge.tokens)
        {
            if (token.training.Count == 0)
            {
                continue;
            }

            token.training = token.training.Select(f => Quantize(f)).Distinct().ToList();
            token.feature = token.training.Aggregate((a, b) => a + b) / token.training.Count;
        }
    }

    /// <summary>
    /// Return a frame containing the token type, touch pattern, estimated 
    /// direction and estimated position
    /// </summary>
    public Frame OrientToken(Token token, TouchPattern pattern)
    {
        Vector2 centroid = (pattern.a + pattern.b + pattern.c) / 3f;
        Vector3 feature = ExtractSidesFeature(pattern);

        float nextAngle;
          
        // direction is from 2nd longest side to shortest side
        if (Triangle.IsLine(token.feature))
        {
            var points = new List<Vector2> { pattern.a, pattern.b, pattern.c }
                        .OrderBy(point => (centroid - point).magnitude)
                        .ToList();

            nextAngle = PolarAngle(points[2] - points[1]);
        }
        // direction is the normal vector from the first side of the feature
        else
        {
            int cycles = Triangle.CountCycleToMatch(feature, token.feature);
        
            Vector2 ab = pattern.b - pattern.a;
            Vector2 bc = pattern.c - pattern.b;
            Vector2 ca = pattern.a - pattern.c;

            Vector2 abN = new Vector2(-ab.y, ab.x);
            Vector2 bcN = new Vector2(-bc.y, bc.x);
            Vector2 caN = new Vector2(-ca.y, ca.x);

            Vector2 normal = Vector2.up;

            if (cycles == 0) normal = abN;
            if (cycles == 1) normal = bcN;
            if (cycles == 2) normal = caN;

            nextAngle = PolarAngle(normal);

            float prevAngle = history.Count > 0 ? history.Last().direction : nextAngle;
            float delta = Mathf.DeltaAngle(prevAngle, nextAngle);

            // if we have a sharp rotation, just fall back to minimising delta
            // angle from previous frame
            if (Mathf.Abs(delta) > 60)
            {
                float abD = Mathf.DeltaAngle(prevAngle, PolarAngle(abN));
                float bcD = Mathf.DeltaAngle(prevAngle, PolarAngle(bcN));
                float caD = Mathf.DeltaAngle(prevAngle, PolarAngle(caN));

                if (Mathf.Abs(abD) < Mathf.Abs(delta))
                {
                    delta = abD;
                    nextAngle = PolarAngle(abN);
                }

                if (Mathf.Abs(bcD) < Mathf.Abs(delta))
                {
                    delta = bcD;
                    nextAngle = PolarAngle(bcN);
                }

                if (Mathf.Abs(caD) < Mathf.Abs(delta))
                {
                    delta = caD;
                    nextAngle = PolarAngle(caN);
                }
            }
        }

        return new Frame
        {
            token = token,
            position = centroid,
            direction = nextAngle,

            pattern = pattern,
        };
    }

    /// <summary>
    /// Return the known token most likely to be responsible for this touch 
    /// pattern. If none are close, return null
    /// </summary>
    public Token ClassifyPattern(TouchPattern pattern)
    {
        Vector3 feature = ExtractSidesFeature(pattern);

        var closest = allTraining.OrderBy(data => Triangle.CompareBestCycle(data.feature, feature))
                                 .Take(16)
                                 .ToList();
        
        var best = knowledge.tokens.OrderByDescending(token => closest.Count(data => data.token == token))
                                   .First();

        return best;
    }
}
