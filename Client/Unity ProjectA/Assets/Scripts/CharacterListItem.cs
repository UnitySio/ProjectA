using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CharacterListItem : MonoBehaviour
{
    public int uid;
    public string name;
    public int grade;
    public int lv;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI gradeText;
    public TextMeshProUGUI lvText;

    public void Start()
    {
        gradeText.text = grade.ToString();
        lvText.text = lv.ToString();
    }
}
