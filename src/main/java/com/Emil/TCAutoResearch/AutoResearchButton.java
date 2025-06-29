package com.Emil.TCAutoResearch;

public class AutoResearchButton extends CustonButton {

    public AutoResearchButton(int id, int xPos, int yPos, int width, int height, String displayString) {
        super(id, xPos, yPos, width, height, displayString);
        this.visible = false;

    }

    public AutoResearchButton(int id, int xPos, int yPos, String displayString) {
        super(id, xPos, yPos, displayString);
        this.visible = false;
    }

}
