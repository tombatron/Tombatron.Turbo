import type {SidebarsConfig} from '@docusaurus/plugin-content-docs';

const sidebars: SidebarsConfig = {
  docsSidebar: [
    'getting-started',
    {
      type: 'category',
      label: 'Tutorials',
      items: [
        'tutorials/todo-list',
        'tutorials/real-time-streams',
      ],
    },
    {
      type: 'category',
      label: 'Guides',
      items: [
        'guides/turbo-frames',
        'guides/turbo-streams',
        'guides/form-validation',
        'guides/authorization',
        'guides/testing',
        'guides/troubleshooting',
        'guides/claude-code',
      ],
    },
    {
      type: 'category',
      label: 'API Reference',
      items: [
        'api/ITurbo',
        'api/ITurboStreamBuilder',
        'api/TagHelpers',
        'api/TurboOptions',
      ],
    },
    {
      type: 'category',
      label: 'Reference',
      items: [
        'reference/configuration',
        'reference/helper-extensions',
        'reference/stimulus',
        'reference/import-maps',
        'reference/minimal-api',
        'reference/source-generator',
      ],
    },
    {
      type: 'category',
      label: 'Migration',
      items: [
        'migration/from-blazor-server',
        'migration/from-htmx',
      ],
    },
    {
      type: 'category',
      label: 'Advanced',
      items: [
        'performance',
        'security',
        'browser-compatibility',
      ],
    },
  ],
};

export default sidebars;
