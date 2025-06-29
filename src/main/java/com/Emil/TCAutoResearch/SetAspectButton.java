package com.Emil.TCAutoResearch;

import net.minecraft.client.Minecraft;
import net.minecraft.entity.player.EntityPlayer;
import net.minecraft.util.StatCollector;

import thaumcraft.api.aspects.Aspect;
import thaumcraft.client.lib.PlayerNotifications;
import thaumcraft.common.Thaumcraft;

public class SetAspectButton extends CustonButton {

    public static boolean Stop;

    public SetAspectButton(int id, int xPos, int yPos, int width, int height, String displayString) {
        super(id, xPos, yPos, width, height, displayString);
        this.visible = false;

    }

    public SetAspectButton(int id, int xPos, int yPos, String displayString) {
        super(id, xPos, yPos, displayString);
        this.visible = false;
    }

    public static Aspect SelectAspect;

    public static void onAction(SetAspectButton button, Minecraft mc, EntityPlayer player,
        GuiResearchTableHelperInterface guiResearchTableMixin, int num) {
        new Thread(new Runnable() {

            @Override
            public void run() {
                var PlayaspectList = Thaumcraft.proxy.getPlayerKnowledge()
                    .getAspectsDiscovered(player.getCommandSenderName());
                boolean Fail = false;
                var AspectNum = PlayaspectList.getAmount(SelectAspect);
                Unless = null;
                Stop = false;
                while (!Stop) {
                    PlayaspectList = Thaumcraft.proxy.getPlayerKnowledge()
                        .getAspectsDiscovered(player.getCommandSenderName());
                    var CurrectNum = PlayaspectList.getAmount(SelectAspect);
                    if (CurrectNum - AspectNum < num) {
                        if (!FindCombineAspect(SelectAspect, player, guiResearchTableMixin)) {
                            Fail = true;
                            break;
                        }
                    } else break;
                    try {
                        Thread.sleep(100);
                    } catch (InterruptedException e) {}
                }
                var CurrectNum = PlayaspectList.getAmount(SelectAspect);

                if (Fail) {
                    PlayerNotifications.addNotification(
                        "合成" + "["
                            + StatCollector.translateToLocal("tc.aspect.help." + SelectAspect.getTag())
                            + "]"
                            + "失败(已经合成"
                            + (CurrectNum - AspectNum)
                            + "),基础元素不足",
                        SelectAspect);
                    if (Unless != null) {
                        PlayerNotifications.addNotification(
                            "请补充" + "["
                                + StatCollector.translateToLocal("tc.aspect.help." + Unless.getTag())
                                + "]"
                                + "元素",
                            Unless);
                    }
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
