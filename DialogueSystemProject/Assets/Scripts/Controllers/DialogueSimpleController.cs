using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueSimpleController : MonoBehaviour
{
    [SerializeField] string _key;
    [SerializeField] float _time = 1f;
    [SerializeField] float _waitTime = 2f;

    private TextMeshProUGUI _textMeshPro;

    void Start()
    {
        DialogueManager.Instance.onDialogueSimpleUpdated += UpdateSimple;
        UpdateSimple();
    }

    void UpdateSimple()
    {
        _textMeshPro.text = DialogueManager.Instance.TextSimple(_key);
    }

    void OnEnable()
    {
        if (_textMeshPro == null)
        {
            _textMeshPro = GetComponent<TextMeshProUGUI>();
        }
        StartCoroutine(EnableAnimation());
    }

    private IEnumerator EnableAnimation()
    {
        float elapsedTime = 0f;
        Color color = _textMeshPro.color;
        color.a = 0f;
        _textMeshPro.color = color;

        while (elapsedTime < _time)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / _time);
            _textMeshPro.color = color;
            yield return null;
        }

        yield return new WaitForSeconds(_waitTime);

        elapsedTime = 0f;
        while (elapsedTime < _time)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(1f - (elapsedTime / _time));
            _textMeshPro.color = color;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
