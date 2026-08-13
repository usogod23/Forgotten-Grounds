using UnityEngine;

public class InteractRaycaster : MonoBehaviour
{
    public PopupController popupController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hitInfo;
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Physics.Raycast(transform.position, transform.forward, out hitInfo, 10f))
            {
                if (hitInfo.collider.CompareTag("Clue"))
                {
                    hitInfo.collider.gameObject.SetActive(false);
                    ClueInfo clueInfo;
                    clueInfo = hitInfo.collider.gameObject.GetComponent<ClueInfo>();
                    if (clueInfo != null)
                    {
                        Debug.Log("We found Clue: " + clueInfo.ClueName);
                        popupController.Show("We found Clue: " + clueInfo.ClueName);
                    }
                } else if (hitInfo.collider.CompareTag("Battery"))
                {
                    hitInfo.collider.gameObject.SetActive(false);
                    Inventory inventory = this.gameObject.GetComponentInParent<Inventory>();
                    inventory.AddBattery();
                    popupController.Show("Added battery to inventory");
                } else if (hitInfo.collider.CompareTag("Pill"))
                {
                    hitInfo.collider.gameObject.SetActive(false);
                    Inventory inventory = this.gameObject.GetComponentInParent<Inventory>();
                    inventory.AddPill();
                    popupController.Show("Added pill to inventory");
                }
            }
        }
        
    }
}
