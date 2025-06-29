package com.Emil.TCAutoResearch.mixins;

import net.minecraft.client.gui.GuiButton;
import net.minecraft.client.gui.GuiTextField;
import net.minecraft.client.gui.inventory.GuiContainer;
import net.minecraft.entity.player.EntityPlayer;
import net.minecraft.inventory.Container;
import net.minecraft.util.ChatComponentText;

import org.spongepowered.asm.mixin.Mixin;
import org.spongepowered.asm.mixin.Shadow;
import org.spongepowered.asm.mixin.gen.Invoker;
import org.spongepowered.asm.mixin.injection.At;
import org.spongepowered.asm.mixin.injection.Inject;
import org.spongepowered.asm.mixin.injection.callback.CallbackInfo;

import com.Emil.TCAutoResearch.*;

import cpw.mods.fml.client.config.GuiButtonExt;
import thaumcraft.api.aspects.Aspect;
import thaumcraft.client.gui.GuiResearchTable;
import thaumcraft.common.Thaumcraft;
import thaumcraft.common.lib.network.PacketHandler;
import thaumcraft.common.lib.network.playerdata.PacketAspectCombinationToServer;
import thaumcraft.common.lib.network.playerdata.PacketAspectPlaceToServer;
import thaumcraft.common.lib.research.ResearchManager;
import thaumcraft.common.lib.research.ResearchNoteData;
import thaumcraft.common.lib.utils.HexUtils;
import thaumcraft.common.tiles.TileResearchTable;

import java.io.IOException;
import java.util.concurrent.atomic.AtomicBoolean;

import static com.Emil.TCAutoResearch.ResearchCurrectNote.PID;
import static java.lang.Thread.sleep;

@Mixin(value = GuiResearchTable.class, remap = false)
public abstract class GuiResearchTableMixin extends GuiContainer implements GuiResearchTableHelperInterface {

    private GuiResearchTableMixin(Container p_i1072_1_) {
        super(p_i1072_1_);
    }

    AutoResearch autoResearch;
    GuiButtonExt button;
    GuiButtonExt button2;
    GuiButtonExt button3;
    GuiButtonExt button4;

    GuiTextField inputField;
    SetAspectButton confirmButton;

    @Shadow
    public ResearchNoteData note = null;
    @Shadow
    EntityPlayer player;
    @Shadow
    private TileResearchTable tileEntity;
    @Shadow
    private static boolean RESEARCHER_1;

    @Override
    public void initGui() {
        super.initGui();
        var PlayaspectList = Thaumcraft.proxy.getPlayerKnowledge()
            .getAspectsDiscovered(player.getCommandSenderName());
        if (PlayaspectList.aspects.size() != Aspect.aspects.size()) {
            button = new GuiButtonExt(101, super.guiLeft - 80, super.guiTop + 255 / 2 - 50, 80, 25, "解锁全部要素");
            this.buttonList.add(button);
        }

        button2 = new GuiButtonExt(
            102,
            super.guiLeft - 80,
            super.guiTop + 255 / 2 - 25,
            80,
            25,
            "自动解锁笔记:" + (Config.AutoResearch ? "开启" : "关闭"));
        button3 = new GuiButtonExt(104, super.guiLeft - 80, super.guiTop + 255 / 2, 80, 25, "解锁当前笔记");
        button3.visible = !Config.AutoResearch;
        button4 = new GuiButtonExt(105, super.guiLeft - 80, super.guiTop + 255 / 2 + 25, 80, 25, "重新解锁上次笔记");

        this.buttonList.add(button2);
        this.buttonList.add(button3);
        this.buttonList.add(button4);

        this.inputField = new GuiTextField(this.fontRendererObj, 0, 0, 25, 10);
        this.inputField.setMaxStringLength(50);
        this.inputField.setFocused(true);
        this.inputField.setVisible(false);
        this.confirmButton = new SetAspectButton(103, 0, 0, 25, 13, "确定");
        this.confirmButton.visible = false;
        this.buttonList.add(this.confirmButton);
        // AutoPlayButton.onAction(player, note, this);
        // autoResearch=new AutoResearch(player,this.mc,this);
        // autoResearch.start();
        if (Config.AutoResearch) {
            autoResearch = new AutoResearch(player, this.mc, this);
            autoResearch.start();
        }
    }

    @Override
    protected void keyTyped(char typedChar, int keyCode) {
        if (this.inputField != null && this.inputField.getVisible()) {
            if (this.inputField.textboxKeyTyped(typedChar, keyCode)) return;
        }
        super.keyTyped(typedChar, keyCode);
    }

    private long lastClickTime = 0;

    @Override
    public void actionPerformed(GuiButton Targetbutton) {
        super.actionPerformed(Targetbutton);
        long now = System.currentTimeMillis();
        if (now - lastClickTime < 200) return; // 忽略200ms内重复点击
        lastClickTime = now;
        if (Targetbutton.id == 101) {
            GetAllAspectButton.onAction(button, mc, player, this);
        } else if (Targetbutton.id == 103) {
            try {
                int num = Integer.parseInt(inputField.getText());
                SetAspectButton.onAction(confirmButton, mc, player, this, num);
            } catch (Exception e) {

            }
        } else if (Targetbutton.id == 102) {
            if (Config.AutoResearch) {
                Config.AutoResearch = false;
                Config.SaveConfiguration();
                button2.displayString = "自动解锁笔记:关闭";
                AutoResearch.Stop = true;
                button3.visible = true;
                button4.visible = true;

            } else {
                Config.AutoResearch = true;
                Config.SaveConfiguration();
                button2.displayString = "自动解锁笔记:开启";
                button3.visible = false;
                button4.visible = false;
                autoResearch = new AutoResearch(player, this.mc, this);
                autoResearch.start();
            }
        } else if (Targetbutton.id == 104) {
            ResearchCurrectNote.ResearchNote(this, player, mc);
        } else if (Targetbutton.id == 105) {
            if (note != null) {
                if (!note.isComplete()) {
                    if (SolvesNote.LastNote != null && !SolvesNote.LastNote.isEmpty()) {
                        if (SolvesNote.LastNoteID.equals(note.key)) {
                            SolvesNote.SolvesNoteHandle(SolvesNote.LastNote);
                        } else mc.thePlayer.addChatMessage(new ChatComponentText("笔记重新解锁失败,记录笔记不同"));
                    } else mc.thePlayer.addChatMessage(new ChatComponentText("笔记重新解锁失败,无上次解锁数据"));
                } else mc.thePlayer.addChatMessage(new ChatComponentText("笔记重新解锁失败,笔记已经解锁"));
            }
        } else mc.thePlayer.addChatMessage(new ChatComponentText("笔记重新解锁失败,没有放入笔记"));

    }

    @Override
    public void onGuiClosed() {
        AutoResearch.Stop = true;
        GetAllAspectButton.Stop = true;
        SetAspectButton.Stop = true;
        if (PID != -1) {
            try {
                Runtime.getRuntime().exec("taskkill /F /PID " + PID);
            } catch (IOException e) {
            }
            PID = -1;
        }
    }
    // @Inject(method = "drawScreen", at = @At("RETURN"))

    // @Inject(method = "drawScreen", at = @At("HEAD"))
    // private void onDrawScreenStart(int mouseX, int mouseY, float partialTicks, CallbackInfo ci) {
    // if (button != null) {
    // button.xPosition = super.guiLeft - 80;
    // button.yPosition = super.guiTop + 10;
    // button.visible = true;
    // }
    // button2.xPosition = super.guiLeft - 80;
    // button2.yPosition = super.guiTop + 35;
    // button2.visible = true;
    // }

    @Invoker("getClickedAspect")
    public abstract Aspect invokegetClickedAspect(int mx, int my, int gx, int gy, boolean ignoreZero);
    // @Inject(method = "mouseClicked", at = @At("HEAD"))

    // @Override
    // public void mouseClicked(int mouseX, int mouseY, int mouseButton)
    @Inject(method = "func_73864_a", at = @At("Tail"), cancellable = false)
    public void mouseClickedRETURN(int mouseX, int mouseY, int mouseButton, CallbackInfo ci) {
        if (isCtrlKeyDown()) {
            int gx = (this.width - this.xSize) / 2;
            int gy = (this.height - this.ySize) / 2;
            Aspect aspect = invokegetClickedAspect(mouseX, mouseY, gx, gy, true);
            if (aspect != null) {
                SetAspectButton.SelectAspect = aspect;
                this.inputField.xPosition = mouseX - 20;
                this.inputField.yPosition = mouseY + 8;
                this.inputField.setVisible(true);
                try {
                    int num = Integer.parseInt(inputField.getText());
                    this.inputField.setText(String.valueOf(num));
                } catch (Exception e) {
                    this.inputField.setText("1");
                }
                this.inputField.setCursorPosition(0);
                this.inputField.setSelectionPos(
                    this.inputField.getText()
                        .length());
                this.confirmButton.xPosition = this.inputField.xPosition + 27;
                this.confirmButton.yPosition = this.inputField.yPosition - 2;
                this.confirmButton.visible = true;
                RESEARCHER_1 = false;

            }
        } else if (this.confirmButton.visible) {
            // super.mouseClicked(mouseX, mouseY, mouseButton);
            RESEARCHER_1 = ResearchManager.isResearchComplete(player.getCommandSenderName(), "RESEARCHER1");
            this.inputField.setVisible(false);
            this.confirmButton.visible = false;
        }
    }

    @Inject(method = "func_73863_a", at = @At("Tail"))
    public void drawScreenTail(int mouseX, int mouseY, float partialTicks, CallbackInfo ci) {
        if (this.inputField != null && this.inputField.getVisible()) {
            this.inputField.drawTextBox();
        }
    }

    public void place(HexUtils.Hex hex, Aspect aspect) {
        PacketHandler.INSTANCE.sendToServer(
            new PacketAspectPlaceToServer(
                this.player,
                (byte) hex.q,
                (byte) hex.r,
                this.tileEntity.xCoord,
                this.tileEntity.yCoord,
                this.tileEntity.zCoord,
                aspect));
    }

    public void combine(Aspect aspect1, Aspect aspect2) {
        PacketHandler.INSTANCE.sendToServer(
            new PacketAspectCombinationToServer(
                this.player,
                this.tileEntity.xCoord,
                this.tileEntity.yCoord,
                this.tileEntity.zCoord,
                aspect1,
                aspect2,
                this.tileEntity.bonusAspects.getAmount(aspect1) > 0,
                this.tileEntity.bonusAspects.getAmount(aspect2) > 0,
                true));
    }

}
