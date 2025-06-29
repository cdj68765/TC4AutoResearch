package com.Emil.TCAutoResearch;

import cpw.mods.fml.client.config.GuiButtonExt;

public class CustonButton extends GuiButtonExt {

    public CustonButton(int id, int xPos, int yPos, int width, int height, String displayString) {
        super(id, xPos, yPos, width, height, displayString);
        this.visible = false;

    }

    public CustonButton(int id, int xPos, int yPos, String displayString) {
        super(id, xPos, yPos, displayString);
        this.visible = false;
    }
    // private static final ResourceLocation BUTTON_TEXTURES = new ResourceLocation("textures/gui/widgets.png");
    ////
    //// @Override
    //// public void drawButton(Minecraft mc, int mouseX, int mouseY) {
    //// if (this.visible) {
    //// FontRenderer fontRenderer = mc.fontRenderer;
    //// mc.getTextureManager().bindTexture(BUTTON_TEXTURES);
    //// GL11.glColor4f(1f, 1f, 1f, 1f);
    ////
    //// boolean hovered = mouseX >= this.xPosition && mouseY >= this.yPosition &&
    //// mouseX < this.xPosition + this.width && mouseY < this.yPosition + this.height;
    ////
    //// int hoverState = getHoverState(hovered);
    ////
    //// // 绘制背景
    //// drawTexturedModalRect(this.xPosition, this.yPosition, 0, 46 + hoverState * 20, this.width / 2, this.height);
    //// drawTexturedModalRect(this.xPosition + this.width / 2, this.yPosition,
    //// 200 - this.width / 2, 46 + hoverState * 20, this.width / 2, this.height);
    ////
    //// // 文本换行
    //// int textPadding = 4; // 内边距
    //// int maxTextWidth = this.width - textPadding * 2;
    ////
    //// List<String> lines = fontRenderer.listFormattedStringToWidth(this.displayString, maxTextWidth);
    ////
    //// // 计算文字总高
    //// int lineHeight = fontRenderer.FONT_HEIGHT;
    //// int totalTextHeight = lines.size() * lineHeight;
    ////
    //// // 计算文字起始 Y 坐标（垂直居中）
    //// int startY = this.yPosition + (this.height - totalTextHeight) / 2;
    ////
    //// for (int i = 0; i < lines.size(); i++) {
    //// String line = lines.get(i);
    //// int strWidth = fontRenderer.getStringWidth(line);
    //// int drawX = this.xPosition + (this.width - strWidth) / 2;
    //// int drawY = startY + i * lineHeight;
    //// fontRenderer.drawString(line, drawX, drawY, 0xFFFFFF);
    //// }
    //// }
    //// }
}
