﻿using UnityEngine;
using System.Collections;



/// <summary>
/// プレイヤーの移動
/// </summary>
public class PlayerMove : MonoBehaviour
{

    //プレイヤーの移動値
    public float m_fMaxSpeed = 15.0f;       //最大速度
    public float m_fAccelSpeed = 5.0f;      //加速値
    public float m_fBrakeSpeed = 5.0f;      //減速値
    public float m_fFriction = 3.5f;      //摩擦値
    public float m_fCurveAngle = 15.0f;      //1回に曲がる角度

    private float m_fNowSpeed;          //現在の速度

    private float m_fBoundSpeed;          //バウンド時の速度
    private Vector3 m_vBoundVec;          //バウンド方向


    //角度制限用変数
    public float m_SlopeMin = 45.0f;        //登れる最低の角度
    public float m_SlopeMax = 60.0f;        //登れる最大の角度
    private float m_fSlopLimit;             //現在の登れる坂

    //Get・Set用変数
    public float PlayerSpeed { get { return m_fNowSpeed; } }
    public float SlopeLimit { get { return m_fSlopLimit; } }
    public float BoundSpeed { get { return m_fBoundSpeed; } }
    public float PlayerMaxSpeed { get { return m_fMaxSpeed; } }
    public Vector3 PlayerDirection { get { return transform.forward; } }

    //判定用変数
    private CharacterController CharaCon;
    private StartProduction StartFlg;    //スタート判定用

    /// <summary>
    /// スタート関数
    /// </summary>
    void Start()
    {
        //取得
        StartFlg = GameObject.Find("StartProduction").GetComponent<StartProduction>();
        if (StartFlg == null)
            Debug.Log("スタート判定取得失敗");

        CharaCon = GetComponent<CharacterController>();
        m_fSlopLimit = m_SlopeMin;  //初期値設定
    }

    /// <summary>
    /// アップデート関数
    /// </summary>
    void Update()
    {
        //---- スタート判定 ----//
        if (StartFlg != null && StartFlg.isStart == false) return;


        //---- アクセル ----//
        if (MultiInput.Instance.GetPressButton(MultiInput.CONTROLLER_BUTTON.CANCEL))
        {
            //加速
            m_fNowSpeed += m_fAccelSpeed * Time.deltaTime;

            //最大速度制限
            if (m_fNowSpeed > m_fMaxSpeed)
                m_fNowSpeed -= m_fBrakeSpeed * Time.deltaTime;
        }

        //---- ブレーキ ----//
        if (MultiInput.Instance.GetPressButton(MultiInput.CONTROLLER_BUTTON.SQUARE))
        {
            //減速
            m_fNowSpeed -= m_fBrakeSpeed * Time.deltaTime;

            //最大速度制限
            if (m_fNowSpeed < -m_fMaxSpeed)
                m_fNowSpeed += m_fAccelSpeed * Time.deltaTime;
        }

        //----   減速   ----//
        //符号保存
        float Sign = Mathf.Sign(m_fNowSpeed);
        //減速
        m_fNowSpeed = Mathf.Abs(m_fNowSpeed) - m_fFriction * Time.deltaTime;

        //0以下にならないように制限
        if (m_fNowSpeed < 0)
            m_fNowSpeed = 0;

        //符号を戻す
        m_fNowSpeed *= Sign;



        //----  曲がる  ----//
        Vector2 StickInput = MultiInput.Instance.GetLeftStickAxis();
        if (m_fNowSpeed != 0)
        {
            Sign = 1;
            if (m_fNowSpeed < 0.0f)
                Sign = -1;

            transform.Rotate(transform.up, Sign * StickInput.x * m_fCurveAngle * Time.deltaTime);
        }

        /*
        //----  右移動  ----//
        if (入力判定)
        {
        }
        //----  左移動  ----//
        if (入力判定)
        {
        }
        */

        //---- 移動させる ----//
        Vector3 vMove;

        //移動量計算
        vMove = transform.forward * m_fNowSpeed;
        CharaCon.Move(vMove * Time.deltaTime);


        //===============================
        // 登れる角度設定
        //===============================
        m_fSlopLimit = Mathf.Ceil(m_fNowSpeed * (m_SlopeMax / m_fMaxSpeed));
        if (m_fSlopLimit < m_SlopeMin)
            m_fSlopLimit = m_SlopeMin;
        if (m_fSlopLimit > m_SlopeMax)
            m_fSlopLimit = m_SlopeMax;
    }


    /// <summary>
    /// バウンド用設定
    /// </summary>
    /// <param name="Vec"></param>
    /// <param name="Speed"></param>
    /*
    public void BoundSet(Vector3 Vec, float Speed, float fSideResistanceValue, float fVerticalResistanceValue)
    {
        //変数設定
        m_fBoundSpeed = Mathf.Abs(Speed);
        m_vBoundVec = Vec;


        //Debug.Log("Side前" + m_fSideSpeed);
        //Debug.Log("Vertical前" + m_fVerticalSpeed);

        //抵抗値で速度減少
        m_fSideSpeed = m_fSideSpeed * fSideResistanceValue;
        m_fVerticalSpeed = m_fVerticalSpeed * fVerticalResistanceValue;
        WeightMove.Attenuation(fVerticalResistanceValue);

        //Debug.Log("Side" + m_fSideSpeed);
        //Debug.Log("Vertical" + m_fVerticalSpeed);
        //Debug.Log("WeightSpeed" + WeightMove.WeightSpeed);

    }
    */
}