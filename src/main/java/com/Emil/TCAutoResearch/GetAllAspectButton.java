package com.Emil.TCAutoResearch;

import net.minecraft.client.Minecraft;
import net.minecraft.entity.player.EntityPlayer;
import net.minecraft.util.StatCollector;

import cpw.mods.fml.client.config.GuiButtonExt;
import thaumcraft.api.aspects.Aspect;
import thaumcraft.common.Thaumcraft;

public class GetAllAspectButton extends CustonButton {

    public GetAllAspectButton(int id, int xPos, int yPos, int width, int height, String displayString) {
        super(id, xPos, yPos, width, height, displayString);
        this.visible = false;

    }

    public GetAllAspectButton(int id, int xPos, int yPos, String displayString) {
        super(id, xPos, yPos, displayString);
        this.visible = false;
    }

    public static boolean Stop = false;

    public static void onAction(GuiButtonExt button, Minecraft mc, EntityPlayer player,
        GuiResearchTableHelperInterface guiResearchTableMixin) {
        new Thread(new Runnable() {

            @Override
            public void run() {
                var PlayaspectList = Thaumcraft.proxy.getPlayerKnowledge()
                    .getAspectsDiscovered(player.getCommandSenderName());
                boolean Fail = false;
                Unless = null;
                Stop = false;
                for (var GetAspect : Aspect.aspects.values()) {
                    int num = 1;
                    if (PlayaspectList.aspects.containsKey(GetAspect)) {
                        continue;
                    }
                    while (!Stop) {
                        PlayaspectList = Thaumcraft.proxy.getPlayerKnowledge()
                            .getAspectsDiscovered(player.getCommandSenderName());
                        var AspectNum = PlayaspectList.getAmount(GetAspect);
                        if (AspectNum < num) {
                            if (!FindCombineAspect(GetAspect, player, guiResearchTableMixin)) {
                                Fail = true;
                                break;
                            }
                        } else break;
                        try {
                            Thread.sleep(500);
                        } catch (InterruptedException e) {}

                    }
                    if (Fail || Stop) break;
                }
                if (Fail) {
                    if (Unless != null) mc.displayGuiScreen(
                        new GuiMessageBox(
                            mc.currentScreen,
                            "全部解锁失败，缺少:[" + StatCollector.translateToLocal("tc.aspect.help." + Unless.getTag())
                                + "]元素，请补充足够基础元素后再重试",
                            () -> {}));
                    else mc
                        .displayGuiScreen(new GuiMessageBox(mc.currentScreen, "全部解锁失败，缺少基础元素，请补充足够基础元素后再重试", () -> {}));

                } else if (!Stop) {
                    mc.displayGuiScreen(new GuiMessageBox(mc.currentScreen, "所有研究元素已经解锁", () -> {}));
                    button.visible = false;
                }
            }
        }).start();

    }

    static Aspect Unless;

    private static boolean FindCombineAspect(Aspect aspect, EntityPlayer player,
        GuiResearchTableHelperInterface guiResearchTableMixin) {
        var Comptent = aspect.getComponents();
        if (Comptent != null) {
            var PlayaspectList = Thaumcraft.proxy.getPlayerKnowledge()
                .getAspectsDiscovered(player.getCommandSenderName());
            if (PlayaspectList.getAmount(Comptent[0]) == 0) {
                if (!FindCombineAspect(Comptent[0], player, guiResearchTableMixin)) return false;
            }
            if (PlayaspectList.getAmount(Comptent[1]) == 0) {
                if (!FindCombineAspect(Comptent[1], player, guiResearchTableMixin)) return false;
            }
            guiResearchTableMixin.combine(Comptent[0], Comptent[1]);
        } else {
            Unless = aspect;
            return false;
        }
        return true;
    }

}
