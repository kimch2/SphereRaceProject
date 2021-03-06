﻿using UnityEngine;
using System.Collections;



/// <summary>
/// プレイヤーの移動
/// </summary>
public class PlayerMove : MonoBehaviour
{
    //移動速度用変数
    public float m_fMaxSpeed = 15.0f;       //最大速度
    public float m_fAccelSpeed = 5.0f;      //加速値
    public float m_fBrakeSpeed = 5.0f;      //減速値
    public float m_fFriction = 3.5f;        //摩擦値
    public float m_fCurveAngle = 15.0f;     //1回に曲がる角度


    private Vector3 m_vMoveDirection;       //進行方向
    private float m_fNowSpeed;              //現在の速度
    private float m_fBoundSpeed;            //バウンド時の速度
    private Vector3 m_vBoundVec;            //バウンド方向


    //角度制限用変数
    public float m_SlopeMin = 45.0f;        //登れる最低の角度
    public float m_SlopeMax = 60.0f;        //登れる最大の角度
    private float m_fSlopLimit;             //現在の登れる坂

    //Get・Set用変数
    public float PlayerSpeed { get { return m_fNowSpeed; } set { m_fNowSpeed = value; } }
    public float SlopeLimit { get { return m_fSlopLimit; } }
    public float BoundSpeed { get { return m_fBoundSpeed; } }
    public float PlayerMaxSpeed { get { return m_fMaxSpeed; } }
    public Vector3 PlayerDirection { get { return m_vMoveDirection; } }

    //判定用変数
    private CharacterController CharaCon;
    private StartProduction StartFlg;    //スタート判定用



    /// <summary>
    /// スタート関数
    /// </summary>
    void Start()
    {
        //スタート判定用オブジェクト取得
        GameObject StartProduction = GameObject.Find("StartProduction");
        
        //データがあった場合のみ処理をする
        if (StartProduction == null)
            Debug.Log("スタート判定取得失敗");
        else
            StartFlg = StartProduction.GetComponent<StartProduction>();

        //キャラクターコントローラー取得
        CharaCon = GetComponent<CharacterController>();

        //初期値設定
        m_fSlopLimit = m_SlopeMin;
        m_vMoveDirection = transform.forward;
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
            Accel();

        //---- ブレーキ ----//
        if (MultiInput.Instance.GetPressButton(MultiInput.CONTROLLER_BUTTON.SQUARE))
            Brake();


        //----   減速   ----//
        m_fNowSpeed = Deceleration(m_fNowSpeed, m_fFriction);     //現在速度
        m_fBoundSpeed = Deceleration(m_fBoundSpeed, m_fFriction);

        //----  曲がる  ----//
        //速度がある時のみ処理を行う
        if (m_fNowSpeed != 0)
            Curve();

        
        //----  右移動  ----//
        Vector3 vSideDirection = Quaternion.Euler(0, 90, 0) * m_vMoveDirection; //進行方向を右に向けたベクトル
        if (MultiInput.Instance.GetPressButton(MultiInput.CONTROLLER_BUTTON.RIGHT_1))
        {
            CharaCon.Move(vSideDirection * m_fAccelSpeed * Time.deltaTime);
        }
        //----  左移動  ----//
        if (MultiInput.Instance.GetPressButton(MultiInput.CONTROLLER_BUTTON.LEFT_1))
        {
            CharaCon.Move(-vSideDirection * m_fAccelSpeed * Time.deltaTime);
        }
        

        //---- 移動させる ----//
        Vector3 vMove;

        //移動量計算
        vMove = m_vMoveDirection * m_fNowSpeed;
        CharaCon.Move(m_vBoundVec * m_fBoundSpeed * Time.deltaTime);
        CharaCon.Move(vMove * Time.deltaTime);


        //プレイヤーを進行方向に向ける
        //transform.rotation = Quaternion.Slerp(transform.rotation, m_vMoveDirection, m_fCurveAngle);
        transform.rotation = Quaternion.LookRotation(m_vMoveDirection);


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
    public void BoundSet(Vector3 Vec, float Speed, Vector3 vResistanceValue)
    {
        //変数設定
        m_fBoundSpeed = Mathf.Abs(Speed);
        m_vBoundVec = Vec;


        //Debug.Log("Side前" + m_fSideSpeed);
        //Debug.Log("Vertical前" + m_fVerticalSpeed);

        //抵抗値で速度減少
        Vector3 vMove = m_vMoveDirection * m_fNowSpeed;
        m_fNowSpeed = (vMove.z * vResistanceValue.z) + (vMove.x * vResistanceValue.x);

        //Debug.Log("Vec" + Vec);

        //Debug.Log("Side" + m_fSideSpeed);
        //Debug.Log("Vertical" + m_fVerticalSpeed);
        //Debug.Log("WeightSpeed" + WeightMove.WeightSpeed);
    }


    /// <summary>
    /// アクセル処理
    /// </summary>
    void Accel()
    {
        //加速
        m_fNowSpeed += m_fAccelSpeed * Time.deltaTime;
        
        //最大速度制限
        if (m_fNowSpeed > m_fMaxSpeed)
            m_fNowSpeed -= m_fBrakeSpeed * Time.deltaTime;
    }


    /// <summary>
    /// ブレーキ処理
    /// </summary>
    void Brake()
    {
        //減速
        m_fNowSpeed -= m_fBrakeSpeed * Time.deltaTime;

        //最大速度制限
        if (m_fNowSpeed < -m_fMaxSpeed)
            m_fNowSpeed += m_fAccelSpeed * Time.deltaTime;
    }


    /// <summary>
    /// 減速
    /// </summary>
    float Deceleration(float Speed, float DecelerationValue)
    {
        //符号保存
        float Sign = Mathf.Sign(Speed);
        //減速
        Speed = Mathf.Abs(Speed) - DecelerationValue * Time.deltaTime;

        //0以下にならないように制限
        if (Speed < 0)
            Speed = 0;

        //符号を戻す
        Speed *= Sign;

        return Speed;
    }

    /// <summary>
    /// カーブ処理
    /// </summary>
    void Curve()
    {
        //入力値取得
        Vector2 StickInput = MultiInput.Instance.GetLeftStickAxis();

        float Sign = 1;
        if (m_fNowSpeed < 0.0f)
           Sign = -1;

        m_vMoveDirection = Quaternion.AngleAxis(Sign * StickInput.x * m_fCurveAngle * Time.deltaTime, transform.up) * m_vMoveDirection;
        //transform.Rotate(transform.up, Sign * StickInput.x * m_fCurveAngle * Time.deltaTime);    
    }
}
