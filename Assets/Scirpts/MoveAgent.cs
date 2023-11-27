using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class MoveAgent : Agent
{
    [SerializeField] private Transform Goal; // Goal
    [SerializeField] private Transform Button; // ��ư
    [SerializeField] float movespeed = 1.0f;
    [SerializeField] Material SuccessMat;
    [SerializeField] Material failMat;
    [SerializeField] Material originalMat;
    [SerializeField] MeshRenderer floorMR;

    private bool GoalActive = false; //���� Ȱ��ȭ �Ǿ����� ����
    private bool buttonActive = false; //��ư�� Ȱ��ȭ �Ǿ����� ����

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition); //player
        sensor.AddObservation(Goal.localPosition); //goal 
        sensor.AddObservation(Button.localPosition); // ��ư

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0]; //Horizontal ���� �޾ƿ�
        float moveZ = actions.ContinuousActions[1]; //Vertical ���� �޾ƿ�

        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * movespeed;
        //�̵��ϸ� �ڵ������� ������ ��ġ ���� ������Ŵ
    }

    private void OnTriggerEnter(Collider other) //�浹�� ��� ����, ���Ǽҵ� END
    {

        if (other.TryGetComponent<Button>(out Button button))
        {
            SetReward(0.2f); // ��ư�� �ε����� +0.2
            GoalActive = true;
            buttonActive = false;
            Button.gameObject.SetActive(false); // ��ư ��Ȱ��ȭ
            Goal.gameObject.SetActive(true); // ��ǥ Ȱ��ȭ
        }

        if (other.TryGetComponent<Goal>(out Goal goal))
        {
            SetReward(1.0f); //�� �ε����� +1 
            StartCoroutine(SwapFloorMaterial(SuccessMat));
            GoalActive = false;
            Goal.gameObject.SetActive(false); // ��ǥ ��Ȱ��ȭ
            EndEpisode();
        }

        if (other.TryGetComponent<Wall>(out Wall wall))
        {
            SetReward(-1.0f); //���� �ε����� -1
            StartCoroutine(SwapFloorMaterial(failMat));
            GoalActive = false;
            Goal.gameObject.SetActive(false); // ��ǥ Ȱ��ȭ
            EndEpisode();
        }

    }

    IEnumerator SwapFloorMaterial(Material mat)
    {
        floorMR.material = mat;
        yield return new WaitForSeconds(0.5f);
        floorMR.material = originalMat;
    }


    public override void OnEpisodeBegin() //���Ǽҵ� �ٽ� ����
    {
        //Goal, Player, ��ư ���� ����
        transform.localPosition = new Vector3(Random.Range(-2.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f)); //Player
        Button.localPosition = new Vector3(Random.Range(-2.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f)); //��ư
        Goal.localPosition = new Vector3(Random.Range(-2.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f)); //��

        if (buttonActive)
        {
            Button.gameObject.SetActive(false); // ��ư ��Ȱ��ȭ
            buttonActive = true; // ���� ���Ǽҵ带 ���� Ȱ��ȭ
        }
        else
        {
            Button.gameObject.SetActive(true); // ��ư Ȱ��ȭ
        }

        if (GoalActive)
        {
            Goal.gameObject.SetActive(true); // ��ǥ Ȱ��ȭ
            GoalActive = false; // ���� ���Ǽҵ带 ���� ��ư ���� ��Ȱ��ȭ
        }
        else
        {
            Goal.gameObject.SetActive(false); // ��ǥ ��Ȱ��ȭ
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> action = actionsOut.ContinuousActions;

        action[0] = Input.GetAxisRaw("Horizontal"); //X��
        action[1] = Input.GetAxisRaw("Vertical"); //Z��
    }
}


