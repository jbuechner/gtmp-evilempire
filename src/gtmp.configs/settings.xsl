<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:param name="userConfig" select="document('settings.user.xml')" />

    <xsl:variable name="root" select="/config" />

    <xsl:template match="/config">
        <xsl:copy>
            <xsl:apply-templates select="@* | node()" />
            <xsl:apply-templates mode="user" select="$userConfig" />
        </xsl:copy>
    </xsl:template>

    <xsl:template match="@* | node()">
        <xsl:copy>
            <xsl:apply-templates select="@* | node()"/>
        </xsl:copy>
    </xsl:template>

    <xsl:template match="/config/*">
        <xsl:variable name="nodeName" select="name(.)" />
        <xsl:if test="not($userConfig/config/*[local-name()=$nodeName])">
            <xsl:copy>
                <xsl:apply-templates select="@* | node()"/>
            </xsl:copy>
        </xsl:if>
    </xsl:template>

    <xsl:template match="/config" mode="user">
        <xsl:param name="root" />
        <xsl:apply-templates select="@* | node()" mode="user" />
    </xsl:template>

    <xsl:template match="/config/*" mode="user">
        <xsl:variable name="nodeName" select="name(.)" />
        <xsl:if test="not($root/*[local-name()=$nodeName])">
            <xsl:copy>
                <xsl:apply-templates select="@* | node()">
                    <xsl:with-param name="root" select="$root" />
                </xsl:apply-templates>
            </xsl:copy>
        </xsl:if>
    </xsl:template>
</xsl:stylesheet>