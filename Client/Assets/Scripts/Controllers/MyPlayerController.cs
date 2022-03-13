using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MyPlayerController : PlayerController
{
	protected override void Init()
	{
		base.Init();
	}

	void LateUpdate()
	{
		Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
	}

	protected override void UpdateController()
	{
		switch (State)
		{
			case CreatureState.Idle:
				GetDirInput();
				break;
			case CreatureState.Moving:
				GetDirInput();
				break;
		}

		base.UpdateController();
	}

	protected override void UpdateIdle()
	{
		// �̵� ���·� ���� Ȯ��
		if (Dir != MoveDir.None)
		{
			State = CreatureState.Moving;
			return;
		}

		// ��ų ���·� ���� Ȯ��
		if (_coSkillCooltime == null && Input.GetKey(KeyCode.Space))
		{
			// 1. �ð��� ��ų�,
			// 2. �ڷ�ƾ�� ���
			// �� �� ��� �� �ϳ��� ����ؼ�, ��ų��� ��Ŷ�� ��Ӻ������ʰ� �ڵ带 ¥����.

			Debug.Log("Skill !");


			C_Skill skill = new C_Skill() { Info = new SkilInfo() };
			skill.Info.SkillID = 1;
			Managers.Network.Send(skill);

			_coSkillCooltime = StartCoroutine("CoInputCoolTime", 0.2f);
		}
	}

	Coroutine _coSkillCooltime;
	IEnumerator CoInputCoolTime(float time)
    {
		yield return new WaitForSeconds(time);
		_coSkillCooltime = null;

	}

	void GetDirInput()
	{
		if (Input.GetKey(KeyCode.W))
		{
			Dir = MoveDir.Up;
		}
		else if (Input.GetKey(KeyCode.S))
		{
			Dir = MoveDir.Down;
		}
		else if (Input.GetKey(KeyCode.A))
		{
			Dir = MoveDir.Left;
		}
		else if (Input.GetKey(KeyCode.D))
		{
			Dir = MoveDir.Right;
		}
		else
		{
			Dir = MoveDir.None;
		}
	}

	protected override void MoveToNextPos()
	{
		if (Dir == MoveDir.None)
		{
			State = CreatureState.Idle;
			CheckUpdatedFlag();
			return;
		}

		Vector3Int destPos = CellPos;

		switch (Dir)
		{
			case MoveDir.Up:
				destPos += Vector3Int.up;
				break;
			case MoveDir.Down:
				destPos += Vector3Int.down;
				break;
			case MoveDir.Left:
				destPos += Vector3Int.left;
				break;
			case MoveDir.Right:
				destPos += Vector3Int.right;
				break;
		}

		if (Managers.Map.CanGo(destPos))
		{
			if (Managers.Object.Find(destPos) == null)
			{
				CellPos = destPos;
			}
		}

		CheckUpdatedFlag();
	}

	protected override void CheckUpdatedFlag()
    {
		if(_updated)
        {
			C_Move movePacket = new C_Move();
			movePacket.PosInfo = PosInfo;
			Managers.Network.Send(movePacket);
			_updated = false;
		}

    }
}
