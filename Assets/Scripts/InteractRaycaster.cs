using UnityEngine;
public class InteractRaycaster : MonoBehaviour
{
    public PopupController popupController;
    public GameObject pickupText;
    public GameObject doorText;
    public float rayLength = 4;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hitInfo;

        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, rayLength)) {
            if (hitInfo.collider.CompareTag("Clue") || hitInfo.collider.CompareTag("Battery") || hitInfo.collider.CompareTag("Pill"))
            {
                pickupText.SetActive(true);
            }
            else
            {
                pickupText.SetActive(false);
            }

            if(hitInfo.collider.CompareTag("Door"))
            {
                doorText.SetActive(true);
            }
            else
            {
                doorText.SetActive(false);
            }
        }
        else
        {
            pickupText.SetActive(false);
            doorText.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Physics.Raycast(transform.position, transform.forward, out hitInfo, rayLength))
            {
                if (hitInfo.collider.CompareTag("Clue"))
                {

                    ClueInfo clueInfo = hitInfo.collider.gameObject.GetComponent<ClueInfo>();

                    if (clueInfo != null)
                    {
                        ClueManager clueManager = this.gameObject.GetComponentInParent<ClueManager>();
                        
                        if (clueManager != null)
                        {
                            clueManager.AddClue(clueInfo);
                        }

                        Debug.Log("We found Clue: " + clueInfo.ClueName);
                        popupController.Show("We found Clue: " + clueInfo.ClueName);
                    }

                    hitInfo.collider.gameObject.SetActive(false);

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
