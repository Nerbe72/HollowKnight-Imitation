using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum GeoAmount
{
    One     = 1,
    Silver  = 10,
    Gold    = 20
}

public class GeoGenerator : MonoBehaviour
{
    [SerializeField] private GameObject m_geoPrefab;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider == null) return;

        if (collider.CompareTag(GameTagMask.Tag(Tags.Attack)))
        {
            for (int i = 0; i< Random.Range(1, 5); i++)
            {
                GameObject obj = Instantiate(m_geoPrefab);
                GeoAmount geoType = GeoAmount.One;
                switch (Random.Range(0, 3))
                {
                    case 0:
                        break;
                    case 1:
                        geoType = GeoAmount.Silver;
                        break;
                    case 2:
                    default:
                        geoType = GeoAmount.Gold;
                        break;
                }

                obj.GetComponent<Geo>().InitGeo(transform.position, geoType);
            }

            CameraManager.instance.ObjectShake(gameObject, 0.01f, 0.2f, 0.05f);
        }

        SetAction(() => {
            useAction(1);
        });

    }

    private UnityAction m_action = null;

    private void SetAction(UnityAction _action)
    {
        m_action = _action;
    }

    private void useAction(int a)
    {

    }
}
