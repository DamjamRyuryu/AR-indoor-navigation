using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ScrollList.Basic;
using UnityEngine.UIElements;

//Ҫ��
// 1��paddingLeft��paddingRight ����Ҫ�ֶ������ˡ�
// ��Ϊ��Ҫ��֤��һ�������һ��Cell���Դ���Viewport�����ġ�
// ���ԣ���Ҫǿ�� paddingLeft = paddingRight = (Viewport��� - Cell���)/2��
// ������㷽ʽ�ԡ�Cell��� <= Viewport��ȡ��͡�Cell��� > Viewport��ȡ�������������õġ�
// ����Ҫע�⣬����Cell��� > Viewport��ȡ�ʱ��paddingLeft �� paddingRight ��Ϊ������ʱ���ұ߽粻����ȫ��ʾ����PageViewֻ�����ó� Loop �������塣

// 2��spacingX �����ִ���ʽ
// ��ʽһ�������û�ָ��
// ��ʽ����ǿ��ָ��Ϊ Viewport��� - Cell��ȣ��� ÿ��Cell��ռһҳ
// 
//    �������
//    ���һ��Cell��� <= Viewport��ȣ��������������ʱ����ǿ�ƣ�
//          paddingLeft = paddingRight = (Viewport��� - Cell���)/2��
//          spacingX = Viewport��� - Cell��ȡ�
//    �������Cell��� > Viewport��ȣ���̫����������ô�����ˣ�
//          ����ʽһ��ͬ���һ�д��������spacingXΪ�������ڻ��ص�ѹס�����û����жԸ��������  
//          ����ʽ����paddingLeft = paddingRight = (Viewport��� - Cell���)/2�� spacingX = 0���ܹ���֤
//          ����ʽ����ǿ�Ƽ�飬��ֹ�������������

// 3���Զ�Snap: ���ֺ�Snap���ɵ�ǰλ�úͷ����ٶȹ�ͬ������
//    ������ʱ����ҳδ�䣬����ݷ����ٶ�ȷ���Ƿ񷭵�ǰһҳ����һҳ��

// 4��spacingX

// 5��loop: �������Ƿ�ѭ����ҳ

// 6��carousel: �������ֲ�

namespace UserI
{
    //2020.4.2 ���ǲ���List�ķ�ʽ������ֱ��ʵ�ֻ����ӿڲ���dotween�����ƶ�
    public class PageView : UserInterface
    {
        ////�����߾�
        //protected override void FixPadding()
        //{
        //    paddingTop = paddingBottom = (viewportRT.rect.height - CellRerf.rect.height) / 2;
        //}

        //�������
        //protected override void FixSpacingY()
        //{
        //    spacingY = viewportRT.rect.width - cellPrefabRT.rect.width;
        //}
    }
}


