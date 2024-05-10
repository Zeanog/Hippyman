using Neo.Utility;
using Neo.Utility.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Neo;
using System.Xml.Schema;
using Neo.StateMachine.Wrappers;

public class Game : EventManager
{
    private static Game instance = null;
    public static Game Instance {
        get {
            if (instance == null)
            {
                var go = GameObject.Find(nameof(Game));
                if (go != null)
                {
                    instance = go.GetComponent<Game>();
                }
                else
                {
                    go = new GameObject(nameof(Game));
                    go.transform.Reset();
                    instance = go.AddComponent<Game>();
                }
            }
            return instance;
        }
    }

    protected List<Adversary> advisories = new();
    public void RegisterAdversary(Adversary adv)
    {
        advisories.Add(adv);
    }

    public void UnregisterAdversary(Adversary adv)
    {
        advisories.Remove(adv);
    }

    //////////////////////////////////////////////////////////////////////////////////

    [SerializeField]
    protected int startGameDelay = 3;

    protected float score = 0f;
    public float Score {
        get {
            return score;
        }

        set {
            if( Mathf.Approximately(score, value))
            {
                return;
            }

            score = value;
            ScoreChanged.Invoke(string.Format("{0}", score.ToString("C")));
        }
    }

    protected Timer         resetAdvisoriesTimer;

    [SerializeField]
    protected               Neo.GridComponent grid;
    public Neo.GridComponent Grid => grid;

    protected AStarPathfinder pathFinder;

    [SerializeField]
    protected Player        player;
    public Player Player => player;

    [SerializeField]
    protected InspectorStateMachine stateMachine;

    [SerializeField]
    protected AnimatedText beginPlayText;

    [Serializable]
    public class ScoreNotificationUnityEvent : UnityEvent<string> {}

    [SerializeField]
    protected ScoreNotificationUnityEvent ScoreChanged;

    [SerializeField]
    protected UnityEvent              LevelComplete;

    public int LayerMaskWall => grid.LayerMaskWall;

    protected override void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        AddListener<EvidenceofCorruption>("OnCollected", OnEvidenceCollected);
        AddListener<Points>("OnCollected", OnPointsCollected);
        AddListener<Money>("OnCollected", OnMoneyCollected);
        AddListener<Pellet>("OnCollected", OnPelletCollected);

        AddListener<Game>("OnGameStart", OnGameStart);
        AddListener<Game>("OnGamePause", OnGamePause);

        pathFinder = new AStarPathfinder(grid);

        LevelComplete.AddListener( () => {
            stateMachine.TriggerEvent("OnLevelComplete");
        } );
    }

    protected virtual void Start()
    {
        int remainingTime = startGameDelay;
        var startTimer = Timer.Create("StartGame", 1f);

        beginPlayText.gameObject.SetActive(true);
        beginPlayText.Text = remainingTime.ToString();
        startTimer.OnElapsed.AddListener( () => {
            if (--remainingTime <= 0 )
            {
                beginPlayText.gameObject.SetActive(false);
                stateMachine.TriggerEvent("OnStartGameTimerComplete");
                GameObject.Destroy(startTimer.gameObject);
            } else
            {
                beginPlayText.Text = remainingTime.ToString();
                
                startTimer.Restart();
            }
        });
        startTimer.Restart();
    }

    public void CreateVisualizationsFor(Path path)
    {
        path.CreateVisualizations(grid);
    }

    public Path FindIfPathExists( Vector3 start, Vector3 dest )
    {
        grid.WorldToGrid(start, out Vector2Int startLoc);
        grid.WorldToGrid(dest, out Vector2Int destLoc);
        var path = pathFinder.FindPath(startLoc, destLoc);
        return path;
    }

    public Path FindIfPathExists(Vector2Int startLoc, Vector2Int destLoc)
    {
        var path = pathFinder.FindPath(startLoc, destLoc);
        return path;
    }

    public virtual void BroadcastGameEvent(string evtName)
    {
        BroadcastEvent(this, evtName, null);
    }

    protected void OnGamePause(object sender, object evtData)
    {
        foreach (var adv in advisories)
        {
            //adv.SetSleeping();
        }
    }

    public void StartGame()
    {
        BroadcastEvent<Game>(this, "OnGameStart", null);
    }

    protected void OnGameStart(object sender, object evtData)
    {
        foreach(var adv in advisories)
        {
            //adv.SetAggressive();
        }
    }

    protected void OnEvidenceCollected(object sender, object evtData)
    {
        float duration = (float)evtData;
        if (resetAdvisoriesTimer == null)
        {
            resetAdvisoriesTimer = Timer.Create("EvidenceCollected", duration);
            resetAdvisoriesTimer.OnElapsed.AddListener(() =>
            {
                BroadcastEvent<EvidenceofCorruption>(this, "OnLost", null);
            });
            resetAdvisoriesTimer.Restart();
        }
        if (resetAdvisoriesTimer.IsRunning)
        {
            resetAdvisoriesTimer.Restart();
        }
    }

    protected void OnPointsCollected(object sender, object evtData)
    {
        Score += Convert.ToSingle(evtData);
    }

    protected void OnMoneyCollected(object sender, object evtData)
    {
        Score += Convert.ToSingle(evtData);
    }

    protected void OnPelletCollected(object sender, object evtData)
    {
        Score += Convert.ToSingle(evtData);

        if(Pellet.NumPellets <= 0)
        {
            LevelComplete?.Invoke();
            RemoveListener<Pellet>("OnCollected", OnPelletCollected);
        }
    }

    public bool GridToWorld(int x, int y, out Vector3 worldPos)
    {
        return grid.GridToWorld(x, y, out worldPos);
    }

    public bool GridToWorld(Vector2Int loc, out Vector3 worldPos)
    {
        return grid.GridToWorld(loc, out worldPos);
    }

    public bool WorldToGrid(Vector3 worldPos, out int x, out int y)
    {
        return grid.WorldToGrid(worldPos, out x, out y);
    }

    public bool WorldToGrid(Vector3 worldPos, out Vector2Int loc)
    {
        return grid.WorldToGrid(worldPos, out loc);
    }

    public void SimulateEvidenceCollected( float duration )
    {
        BroadcastEvent<EvidenceofCorruption>(this, "OnCollected", duration);
    }

    public void SimulateEvidenceLost()
    {
        BroadcastEvent<EvidenceofCorruption>(this, "OnLost", null);
    }

    public void SimulateGoToJail()
    {
        BroadcastEvent<Adversary>(this, "OnGoToJail", null);
    }
}