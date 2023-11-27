using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class MoveAgent : Agent
{
    [SerializeField] private Transform Goal; // Goal
    [SerializeField] private Transform Button; // 버튼
    [SerializeField] float movespeed = 1.0f;
    [SerializeField] Material SuccessMat;
    [SerializeField] Material failMat;
    [SerializeField] Material originalMat;
    [SerializeField] MeshRenderer floorMR;

    private bool GoalActive = false; //골이 활성화 되었는지 여부
    private bool buttonActive = false; //버튼이 활성화 되었는지 여부

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition); //player
        sensor.AddObservation(Goal.localPosition); //goal 
        sensor.AddObservation(Button.localPosition); // 버튼

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0]; //Horizontal 값을 받아옴
        float moveZ = actions.ContinuousActions[1]; //Vertical 값을 받아옴

        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * movespeed;
        //이동하면 자동적으로 들어오는 위치 값을 누적시킴
    }

    private void OnTriggerEnter(Collider other) //충돌할 경우 보상, 에피소드 END
    {

        if (other.TryGetComponent<Button>(out Button button))
        {
            SetReward(0.2f); // 버튼에 부딪히면 +0.2
            GoalActive = true;
            buttonActive = false;
            Button.gameObject.SetActive(false); // 버튼 비활성화
            Goal.gameObject.SetActive(true); // 목표 활성화
        }

        if (other.TryGetComponent<Goal>(out Goal goal))
        {
            SetReward(1.0f); //골에 부딪히면 +1 
            StartCoroutine(SwapFloorMaterial(SuccessMat));
            GoalActive = false;
            Goal.gameObject.SetActive(false); // 목표 비활성화
            EndEpisode();
        }

        if (other.TryGetComponent<Wall>(out Wall wall))
        {
            SetReward(-1.0f); //벽에 부딪히면 -1
            StartCoroutine(SwapFloorMaterial(failMat));
            GoalActive = false;
            Goal.gameObject.SetActive(false); // 목표 활성화
            EndEpisode();
        }

    }

    IEnumerator SwapFloorMaterial(Material mat)
    {
        floorMR.material = mat;
        yield return new WaitForSeconds(0.5f);
        floorMR.material = originalMat;
    }


    public override void OnEpisodeBegin() //에피소드 다시 시작
    {
        //Goal, Player, 버튼 랜덤 생성
        transform.localPosition = new Vector3(Random.Range(-2.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f)); //Player
        Button.localPosition = new Vector3(Random.Range(-2.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f)); //버튼
        Goal.localPosition = new Vector3(Random.Range(-2.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f)); //골

        if (buttonActive)
        {
            Button.gameObject.SetActive(false); // 버튼 비활성화
            buttonActive = true; // 다음 에피소드를 위해 활성화
        }
        else
        {
            Button.gameObject.SetActive(true); // 버튼 활성화
        }

        if (GoalActive)
        {
            Goal.gameObject.SetActive(true); // 목표 활성화
            GoalActive = false; // 다음 에피소드를 위해 버튼 상태 비활성화
        }
        else
        {
            Goal.gameObject.SetActive(false); // 목표 비활성화
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> action = actionsOut.ContinuousActions;

        action[0] = Input.GetAxisRaw("Horizontal"); //X축
        action[1] = Input.GetAxisRaw("Vertical"); //Z축
    }
}


