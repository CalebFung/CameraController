/**
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour 
{
    public Transform player;

    private Camera m_mainCamera;

    private Vector3 initialCameraPosition;
    private Quaternion initialCameraRotation;
    private Matrix4x4 initialProjectionMatrix;
    private Matrix4x4 ortho;
    private Matrix4x4 perspective;
    private float fov = 60f;
    private float near = 0.3f;
    private float far = 1000f;
    private float orthographicSize = 4f;
    private float aspect;
    private bool orthoOn;

    private Vector3 initalCameraPos;
    private bool rotating = false;
    private enum Targets {
        PLAYER,
        SOURCE
    }
    private Targets panTarget = Targets.PLAYER;
    private bool followTarget = false;

    private Vector3 offset = new Vector3(0, 0, -3);

    private void Start()
    {
        m_mainCamera = Camera.main;
        initalCameraPos = new Vector3(m_mainCamera.transform.position.x, m_mainCamera.transform.position.y, m_mainCamera.transform.position.z);

        aspect = (float)Screen.width / (float)Screen.height;
        ortho = Matrix4x4.Ortho(-orthographicSize * aspect, orthographicSize * aspect, -orthographicSize, orthographicSize, near, far);
        perspective = Matrix4x4.Perspective(fov, aspect, near, far);
        m_mainCamera.projectionMatrix = perspective;
        orthoOn = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            orthoOn = !orthoOn;
            if (orthoOn)
            {
                panTarget = Targets.PLAYER;
                RotateCamera(new Vector3(0, 0, 0), 1f);
                BlendToMatrix(ortho, 1f);
                FollowTarget(player.position + offset, 0.3f);
            }
            else
            {
                panTarget = Targets.SOURCE;
                followTarget = false;
                Debug.Log(initalCameraPos);
                FollowTarget(initalCameraPos, 0.3f);
                BlendToMatrix(perspective, 1f);
                RotateCamera(new Vector3(30, 0, 0), 1f);
            }
        }
    }

    private void LateUpdate()
    {
        if (followTarget)
        {
            Vector3 finalPosition = player.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(m_mainCamera.transform.position, finalPosition, 1);
            m_mainCamera.transform.position = smoothedPosition;

            m_mainCamera.transform.LookAt(player);
        }
    }


    private IEnumerator RotateObject(GameObject _gameObject, Quaternion newRotation, float duration)
    {
        if (panTarget == Targets.SOURCE)
        {
            yield return new WaitForSeconds(0.3f);
        }

        if (rotating)
        {
            yield break;
        }
        rotating = true;

        Quaternion currentRot = _gameObject.transform.rotation;

        float counter = 0;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            _gameObject.transform.rotation = Quaternion.Lerp(currentRot, newRotation, counter / duration);
            yield return null;
        }
        rotating = false;
    }

    private Coroutine RotateCamera(Vector3 rotationTo, float duration)
    {
        Quaternion newRotation = Quaternion.Euler(rotationTo);
        return StartCoroutine(RotateObject(m_mainCamera.gameObject, newRotation, duration));
    }

    private static Matrix4x4 MatrixLerp(Matrix4x4 from, Matrix4x4 to, float time)
    {
        Matrix4x4 ret = new Matrix4x4();
        for (int i = 0; i < 16; i++)
            ret[i] = Mathf.Lerp(from[i], to[i], time);
        return ret;
    }

    private IEnumerator LerpFromTo(Matrix4x4 src, Matrix4x4 dest, float duration)
    {
        if (panTarget == Targets.SOURCE)
        {
            yield return new WaitForSeconds(0.3f);
        }

        float startTime = Time.time;
        while (Time.time - startTime < duration)
        {
            m_mainCamera.projectionMatrix = MatrixLerp(src, dest, (Time.time - startTime) / duration);
            yield return 1;
        }
        m_mainCamera.projectionMatrix = dest;
    }

    private Coroutine BlendToMatrix(Matrix4x4 targetMatrix, float duration)
    {
        return StartCoroutine(LerpFromTo(m_mainCamera.projectionMatrix, targetMatrix, duration));
    }

    private IEnumerator _FollowTarget(Vector3 endPos, float duration)
    {
        if (panTarget == Targets.PLAYER)
        {
            yield return new WaitForSeconds(1f);
        }

        float counter = 0;
        Vector3 startingPos = m_mainCamera.transform.position;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            m_mainCamera.gameObject.transform.position = Vector3.Lerp(startingPos, endPos, counter / duration);
            yield return null;
        }
        if (panTarget == Targets.PLAYER)
        {
            followTarget = true;
        }
    }

    private Coroutine FollowTarget(Vector3 endPos, float duration)
    {
        return StartCoroutine(_FollowTarget(endPos, duration));
    }
}
