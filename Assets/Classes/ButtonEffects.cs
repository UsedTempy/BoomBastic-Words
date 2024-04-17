using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonEffects : MonoBehaviour {
    public bool IsHovering = false;

    public void PointerEnter(GameObject Top) {
        LeanTween.cancel(Top);
        LeanTween.moveLocalY(Top, 25f, .1f);
        IsHovering = true;
    }

    public void PointerExited(GameObject Top) {
        LeanTween.cancel(Top);
        LeanTween.moveLocalY(Top, 15f, .15f);
        IsHovering = false;
    }

    public void PointerDown(GameObject Top) {
        LeanTween.cancel(Top);
        LeanTween.moveLocalY(Top, 6f, .05f).setEase(LeanTweenType.easeInBack);
    }

    public void PointerUp(GameObject Top) {
        float yFloat;

        if (IsHovering) {
            yFloat = 25f;
        } else {
            yFloat = 15f;
        }

        LeanTween.cancel(Top);
        LeanTween.moveLocalY(Top, yFloat, .1f).setEase(LeanTweenType.easeOutBounce);
    }


    public void PointerEnterImage(GameObject Image) {
        LeanTween.cancel(Image);
        LeanTween.scale(Image, new Vector3(1.1f, 1.1f, 1.1f), .1f);
        IsHovering = true;
    }

    public void PointerExitedImage(GameObject Image) {
        LeanTween.cancel(Image);
        LeanTween.scale(Image, new Vector3(1f, 1f, 1f), .06f);
        IsHovering = false;
    }

    public void PointerDownImage(GameObject Image) {
        LeanTween.cancel(Image);
        LeanTween.scale(Image, new Vector3(0.85f,0.85f, 0.85f), .04f);
    }
    public void PointerUpImage(GameObject Image) {
        LeanTween.cancel(Image);
        float yFloat;

        if (IsHovering) {
            yFloat = 1.1f;
        } else {
            yFloat = 1f;
        }

        LeanTween.scale(Image, new Vector3(yFloat, yFloat, yFloat), .04f);
    }
}
