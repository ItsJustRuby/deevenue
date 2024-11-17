FROM dbeaver/cloudbeaver:24.2

COPY data-sources.json /opt/cloudbeaver/workspace/GlobalConfiguration/.dbeaver/
