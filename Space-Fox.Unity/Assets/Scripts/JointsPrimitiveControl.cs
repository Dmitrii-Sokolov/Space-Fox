using UnityEngine;

public class JointsPrimitiveControl : MonoBehaviour
{
  [SerializeField] private float mForce = 300f;
  [SerializeField] private float mSpeed = 100f;

  [SerializeField] private HingeJoint mLeftForward = default;
  [SerializeField] private HingeJoint mLeftBack = default;

  [SerializeField] private HingeJoint mRightForward = default;
  [SerializeField] private HingeJoint mRightBack = default;

  private void Start()
  {
    mLeftForward.useMotor = true;
    mLeftBack.useMotor = true;
    mRightForward.useMotor = true;
    mRightBack.useMotor = true;
  }

  // Update is called once per frame
  private void Update()
  {
    if (Input.GetKey(KeyCode.A))
    {
      mLeftForward.motor =
      mLeftBack.motor =
      GetMotor(false);

      mRightForward.motor =
      mRightBack.motor =
      GetMotor(true);
    }
    else if (Input.GetKey(KeyCode.D))
    {
      mLeftForward.motor =
      mLeftBack.motor =
      GetMotor(true);

      mRightForward.motor =
      mRightBack.motor =
      GetMotor(false);
    }
    else if (Input.GetKey(KeyCode.W))
    {
      mLeftForward.motor =
      mLeftBack.motor =
      mRightForward.motor =
      mRightBack.motor =
      GetMotor(true);
    }
    else if (Input.GetKey(KeyCode.S))
    {
      mLeftForward.motor =
      mLeftBack.motor =
      mRightForward.motor =
      mRightBack.motor =
      GetMotor(false);
    }
    else
    {
      mLeftForward.motor =
      mLeftBack.motor =
      mRightForward.motor =
      mRightBack.motor =
      GetNullMotor();
    }
  }

  private JointMotor GetMotor(bool direction)
  {
    return new JointMotor()
    {
      force = mForce,
      targetVelocity = direction ? mSpeed : -mSpeed
    };
  }

  private JointMotor GetNullMotor()
  {
    return new JointMotor()
    {
      force = 0f,
      targetVelocity = 0f
    };
  }
}
