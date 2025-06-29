package com.Emil.TCAutoResearch;

import net.minecraft.client.gui.GuiButton;
import net.minecraft.client.gui.GuiScreen;

import java.io.IOException;
import java.util.List;

public class GuiMessageBox extends GuiScreen {

    private final String message;
    private final Runnable onConfirm;
    private Runnable offConfirm;

    private final GuiScreen parent;
    private long pid=-1;
    public GuiMessageBox(GuiScreen parent, String message, Runnable onConfirm) {
        this.message = message;
        this.onConfirm = onConfirm;
        this.parent = parent;
    }
    public GuiMessageBox(GuiScreen parent, String message,long PID,Runnable onConfirm,Runnable offConfirm) {
        this.message = message;
        this.onConfirm = onConfirm;
        this.offConfirm = offConfirm;
        this.parent = parent;
        this.pid=PID;
    }

    @Override
    public void initGui() {
        int centerX = this.width / 2;
        int centerY = this.height / 2;

        this.buttonList.clear();
        this.buttonList.add(new GuiButton(0, centerX - 60, centerY + 10, 50, 20, "是"));
        this.buttonList.add(new GuiButton(1, centerX + 10, centerY + 10, 50, 20, "否"));
    }

    @Override
    protected void actionPerformed(GuiButton button) {
        if (button.id == 0)
        {
            if (onConfirm != null && pid != -1) onConfirm.run();
        } else if (button.id == 1&&offConfirm!=null)
        {
            offConfirm.run();
        }
        this.mc.displayGuiScreen(parent); // 回到之前的界面
    }

    @Override
    public void drawScreen(int mouseX, int mouseY, float partialTicks) {
        this.drawDefaultBackground();

        int centerX = this.width / 2;
        int centerY = this.height / 2;

        //this.drawCenteredString(this.fontRendererObj, message, centerX, centerY - 20, 0xFFFFFF);

        List<String> lines = this.fontRendererObj.listFormattedStringToWidth(message, 250); // 限宽250像素
        int y = this.height / 2 - lines.size() * (this.fontRendererObj.FONT_HEIGHT + 2) / 2;
        for (String line : lines) {
            this.drawCenteredString(this.fontRendererObj, line, this.width / 2, y, 0xFFFFFF);
            y += this.fontRendererObj.FONT_HEIGHT + 2;
        }

        super.drawScreen(mouseX, mouseY, partialTicks);
    }

    @Override
    public boolean doesGuiPauseGame() {
        return false;
    }
}
