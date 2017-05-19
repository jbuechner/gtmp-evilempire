<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:param name="userConfig" select="document('settings.user.xml')" />
  <xsl:template match="/config">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />
      <xsl:apply-templates select="$userConfig/config/*" mode="user" />
    </xsl:copy>
  </xsl:template>
  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="@* | node()" mode="user">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" mode="user" />
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
  <xsl:template match="/config/resource">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" mode="user" />
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>