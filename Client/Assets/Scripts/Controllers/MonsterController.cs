using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : BaseController
{
    Coroutine _coPatrol;
    Vector3Int _destCellPos;
    
    public override State CurrState
    {
        get { return _state; }
        set
        {
            if (_state == value)
                return;
            
            base._state = value;
            UpdateAnimation();
            if (_coPatrol != null)
            {
                StopCoroutine("CoPatrol");
                _coPatrol = null;
            }
        }
    }
    
    protected override void Init()
    {
        base.Init();
        
        CurrState = State.Idle;
        Dir = MoveDir.None;
    }

    protected override void UpdateIdle()
    {
        base.UpdateIdle();
        if (_coPatrol == null)
            _coPatrol = StartCoroutine("CoPatrol");
    }

    protected override void MoveToNextPos()
    {
        Vector3Int moveCellDir = _destCellPos - CellPos;
        // TODO : Astar
        if (moveCellDir.x > 0)
            Dir = MoveDir.Right;
        else if (moveCellDir.x < 0)
            Dir = MoveDir.Left;
        else if (moveCellDir.y > 0)
            Dir = MoveDir.Up;
        else if (moveCellDir.y < 0)
            Dir = MoveDir.Down;
        else
            Dir = MoveDir.None;
        
        Vector3Int destPos = CellPos;
        switch (_moveDir)
        {
            case MoveDir.Up:
                destPos += Vector3Int.up;
                break;
            case MoveDir.Left:
                destPos += Vector3Int.left;
                break;
            case MoveDir.Right:
                destPos += Vector3Int.right;
                break;
            case MoveDir.Down:
                destPos += Vector3Int.down;
                break;
        }

        if (Managers.Map.CanGo(destPos) && Managers.Object.Find(destPos) == null)
        {
            CellPos = destPos;
        }
        else
        {
            CurrState = State.Idle;
        }
    }

    IEnumerator CoPatrol()
    {
        int waitSeconds = Random.Range(1, 4);
        yield return new WaitForSeconds(waitSeconds);

        for (int i = 0; i < 10; i++)
        {
            int xRange = Random.Range(-5, 6);
            int yRange = Random.Range(-5, 6);

            Vector3Int randPos = CellPos + new Vector3Int(xRange, yRange, 0);

            if (Managers.Map.CanGo(randPos) && Managers.Object.Find(randPos) == null)
            {
                _destCellPos = randPos;
                CurrState = State.Moving;
                yield break;
            }
        }

        CurrState = State.Idle;
    }

    public override void OnDamaged()
    {
        // TEMP
        GameObject effect = Managers.Resource.Instantiate("Effect/DieEffect");
        effect.transform.position = transform.position;
        effect.GetComponent<Animator>().Play("START");
        GameObject.Destroy(effect, 0.5f);
                    
        Managers.Object.Remove(gameObject);
        Managers.Resource.Destroy(gameObject);
    }
    
    
}